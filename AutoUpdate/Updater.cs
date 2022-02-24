using AutoUpdate.Models;
using AutoUpdate.Package;
using AutoUpdate.Provider;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace AutoUpdate
{
    internal class Updater : IUpdater
    {
        public const string FILENAME = "package_versions.json";

        public static HttpClient HTTPClient;
        public static PackageUpdateEnum PackageUpdateType = PackageUpdateEnum.InPlace;

        public event EventHandler<ProgressDownloadEvent> OnDownloadProgress;
        public event EventHandler<ProgressUploadEvent> OnUploadProgress;

        public string FolderPath { get; private set; }
        public string VersionFile { get; private set; }
        public PackageVersionsObject Versions { get; private set; }

        private readonly IVersionProvider local;
        private readonly IVersionProvider remote;
        private readonly IPackage package;
        private readonly ILogger logger;
        private readonly PrepareHandler prepare;
        private readonly AppSettingsHandler appSettingsHandler;
        private readonly IEnumerable<string> _ignoredFilenames;

        public Updater(IVersionProvider local, IVersionProvider remote, IPackage package, PackageUpdateEnum updateType, HttpClient client, ILogger logger)
        {
            this.local = local;
            this.remote = remote;
            this.package = package;
            PackageUpdateType = updateType;
            HTTPClient = client ?? new();
            this.logger = logger;

            var exe = Process.GetCurrentProcess().MainModule.FileName;
            FolderPath = Path.GetDirectoryName(exe);

            VersionFile = $"{FolderPath}/../{FILENAME}";
            Versions = JsonHelper.Read<PackageVersionsObject>(VersionFile);
            prepare = new PrepareHandler(FolderPath, logger);
            appSettingsHandler = new AppSettingsHandler(FolderPath);
        }

        public Updater(IVersionProvider local, IVersionProvider remote, IPackage package, PackageUpdateEnum updateType, HttpClient client, ILogger logger, IEnumerable<string> ignoringFilenames)
        {
            this.local = local;
            this.remote = remote;
            this.package = package;
            PackageUpdateType = updateType;
            HTTPClient = client ?? new();
            this.logger = logger;
            _ignoredFilenames = ignoringFilenames;

            var exe = Process.GetCurrentProcess().MainModule.FileName;
            FolderPath = Path.GetDirectoryName(exe);

            VersionFile = $"{FolderPath}/../{FILENAME}";
            Versions = JsonHelper.Read<PackageVersionsObject>(VersionFile);
            prepare = new PrepareHandler(FolderPath, logger);
            appSettingsHandler = new AppSettingsHandler(FolderPath);
        }


        public async Task<bool> UpdateAvailableAsync(Func<Version, Version, bool> updateMessageContinue=null)
        {
            var localVersion = await GetLocalVersion();
            var remoteVersion = await GetRemoteVersion();

            // catch failed remote version
            if(VersionIsFailing(remoteVersion))
            {
                logger.LogWarning($"Skip version:{remoteVersion} as it failed previously.");
                return false;
            }

            logger.LogDebug($"Local version:{localVersion}");
            logger.LogDebug($"Remote version:{remoteVersion}");

            if (remoteVersion > localVersion)
            {
                bool continueUpdate = true;
                if (updateMessageContinue != null)
                {
                    continueUpdate = updateMessageContinue.Invoke(localVersion, remoteVersion);
                }

                return continueUpdate;
            }


            return false;
        }

        private bool VersionIsFailing(Version version)
        {
            string vname = PackageUtils.GetVersionString(version);
            return Versions.FailedVersions.Contains(vname);
        }


        public async Task<int> Update(EventHandler<ProgressDownloadEvent> onDownloadProgress=null)
        {
            OnDownloadProgress = onDownloadProgress ?? OnDownloadProgress;
            if (OnDownloadProgress != null)
            {
                OnDownloadProgress.Invoke(this, new("downloading", -1));
            }

            var remoteVersion = await GetRemoteVersion();
            var package = await this.package.GetContentAsync(remoteVersion, OnDownloadProgress);

            // set new version
            var success = SetVersion(package, remoteVersion, OnDownloadProgress);
            if(!success)
            {
                logger.LogError("Remote version do not match with .EXE version!");
                return -1;
            }

            return 0;
        }

        private bool SetVersion(byte[] package, Version version, EventHandler<ProgressDownloadEvent> onDownloadProgress = null)
        {
            var exe = Process.GetCurrentProcess().MainModule.FileName;
            var exename = Path.GetFileName(exe);
            var remoteExe = $"{Path.GetTempPath()}RemoteVersion.exe";

            // save remote version EXE
            // TODO: Set into Memory (now set file into temp folder)
            var archive = new ZipArchive(new MemoryStream(package));
            foreach (ZipArchiveEntry entry in archive.Entries) 
                if (entry.Name == exename) // if (entry.Name.EndsWith(".exe"))
                    entry.ExtractToFile(remoteExe, true);

            // check zip version == exe version
            var versionInfo = FileVersionInfo.GetVersionInfo(remoteExe);
            var newVersion = PackageUtils.GetVersionString(versionInfo);
            var oldVersion = PackageUtils.GetVersionString(version);

            // remove local `RemoteVersion.exe` file
            if (File.Exists(remoteExe)) File.Delete(remoteExe);

            // return version
            if (oldVersion != newVersion)
            {
                SaveFailedVersion(oldVersion);
                return false;
            }

            // set files
            FolderPath = PackageUpdateType switch
            {
                PackageUpdateEnum.SideBySide => $"{FolderPath}/{newVersion}",
                PackageUpdateEnum.InPlace    => $"{FolderPath}",
                _ => throw new MissingMemberException($"Failed not found {PackageUpdateType}"),
            };

            // handle special cases
            try
            {
                (var newFilename, var newContent) = appSettingsHandler.GetFromArchive(archive, _ignoredFilenames, logger);

                // save path
                PackageUtils.ExtractArchive(archive, FolderPath, onDownloadProgress);

                // set special cases file
                appSettingsHandler.SetFile($"{FolderPath}/{newFilename}", newContent);

                // save return
                SaveVersion(oldVersion);
            }
            catch (Exception ex)
            {
                logger.LogCritical($"AutoUpdate: Save new version ERROR: {ex.Message}");
                throw;
            }

            return true;
        }


        private void SaveVersion(string version)
        {
            Versions.Versions.Add(version);
            JsonHelper.Write(Versions, FILENAME);
        }

        private void SaveFailedVersion(string version)
        {
            Versions.FailedVersions.Add(version);
            JsonHelper.Write(Versions, FILENAME);
        }


        public int Restart(Func<List<string>> extraArguments = null, bool hasPrepareTimeThreshold = true)
        {
            var exeFile = Process.GetCurrentProcess().MainModule.FileName;

            // Run: (pre/post)-install.* (.bat / .cmd /.ps /.exe) bestanden
            var exitCode = prepare.RunPreAndPostInstall(hasPrepareTimeThreshold);
            if (exitCode != 0) return exitCode;

            // In-Place (child process)
            var psi = new ProcessStartInfo
            {
                FileName = $"{FolderPath}\\{Path.GetFileName(exeFile)}",
                WorkingDirectory = FolderPath
            };

            // all project arguments
            var args = new ArgumentsContext(extraArguments);
            args.SetAsCollection(psi.ArgumentList);

            // start new process
            logger.LogInformation($"[START NEW PROCESS] {psi.FileName}");

            var process = Process.Start(psi);
            process.WaitForExit();

            return process.ExitCode;
        }


        public async Task<bool> PublishAvailableAsync(Func<Version, Version, bool> publishMessageContinue=null)
        {
            var localVersion = await GetLocalVersion();
            var remoteVersion = await GetRemoteVersion();

            logger.LogDebug($"Local version:{localVersion}");
            logger.LogDebug($"Remote version:{remoteVersion}");

            bool publishing = false;
            if (remoteVersion < localVersion)
            {
                publishing = true;
                if (publishMessageContinue != null)
                {
                    publishing = publishMessageContinue.Invoke(localVersion, remoteVersion);
                }
            }

            return publishing;
        }

        public async Task Publish(EventHandler<ProgressUploadEvent> onUploadProgress=null)
        {
            OnUploadProgress = onUploadProgress ?? OnUploadProgress;
            if (OnUploadProgress != null)
            {
                OnUploadProgress.Invoke(this, new("uploading", -1));
            }

            var localVersion = await GetLocalVersion();
            var exeFile = Process.GetCurrentProcess().MainModule.FileName;
            var exePath = Path.GetDirectoryName(exeFile);
            var currVersion = await CurrentVersionToZipAsync(exePath);

            if(currVersion == null)
            {
                logger.LogError($"[ERROR] Get Zip from {exePath} is nulll");
            }

            await package.SetContentAsync(currVersion, localVersion, OnUploadProgress);
            await SetRemoteVersionAsync(localVersion);
        }


        private async Task SetRemoteVersionAsync(Version version) => await remote.SetVersionAsync(version);

        private static async Task<byte[]> CurrentVersionToZipAsync(string folderName)
        {
            // TODO: Set generated zip into Memory.
            var zipName = $"{Path.GetTempPath()}CurrentVersionToZip.zip";

            // create file
            ZipFile.CreateFromDirectory(folderName, zipName);

            // read file
            var bytes = await File.ReadAllBytesAsync(zipName);

            // delete file
            File.Delete(zipName);

            return bytes;
        }

        public async Task<Version> GetLocalVersion() => await local.GetVersionAsync();

        public async Task<Version> GetRemoteVersion() => await remote.GetVersionAsync();

        public async Task Execute(Func<bool, int, Task> response, bool inDevMode = false)
        {
            if (inDevMode)
            {
                logger.LogWarning("AutoUpdate: In Development mode, disabled Updating!");
            }

            var exitCode = 0;
            var updatable = !inDevMode && await UpdateAvailableAsync();
            if (updatable)
            {
                logger.LogInformation("AutoUpdate: found new version. Updating...");
                exitCode = await Update();
                if (exitCode == 0)
                {
                    exitCode = Restart();
                }
            }
            else
            {
                logger.LogInformation("AutoUpdate: no update found.");
                exitCode = 0;
            }

            await response(updatable, exitCode);
        }

    }
}
