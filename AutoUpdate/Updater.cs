using AutoUpdate.Models;
using AutoUpdate.Package;
using AutoUpdate.Provider;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
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
        private readonly PrepareHandler prepare;

        //TODO include logger from DI
        //private readonly ILogger<Updater> logger;

        public Updater(IVersionProvider local, IVersionProvider remote, IPackage package, PackageUpdateEnum type, HttpClient client)
        {
            this.local = local;
            this.remote = remote;
            this.package = package;
            PackageUpdateType = type;
            HTTPClient = client ?? new();

            var exe = Process.GetCurrentProcess().MainModule.FileName;
            FolderPath = Path.GetDirectoryName(exe);

            VersionFile = $"{FolderPath}/../{FILENAME}";
            Versions = JsonHelper.Read<PackageVersionsObject>(VersionFile);
            prepare = new PrepareHandler(FolderPath);
        }

        public async Task<bool> UpdateAvailableAsync(Func<Version, Version, bool> updateMessageContinue=null)
        {
            var localVersion = await GetLocalVersion();
            var remoteVersion = await GetRemoteVersion();

            // catch failed remote version
            if(VersionIsFailing(remoteVersion))
            {
                Console.WriteLine($"[WARNING] Skip version:{remoteVersion} as it failed previously.");
                return false;
            }

            // Console.WriteLine($"[local version:{localVersion} <-> remote version:{remoteVersion}]");

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
                Console.WriteLine("[ERROR] remote version do not match with .EXE version!");
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
                if (entry.Name == exename) 
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
                PackageUpdateEnum.SideBySide => $"{FolderPath}/../{newVersion}",
                PackageUpdateEnum.InPlace    => $"{FolderPath}",
                _ => throw new MissingMemberException($"Failed not found {PackageUpdateType}"),
            };

            // save path
            PackageUtils.ExtractArchive(archive, FolderPath, onDownloadProgress);

            SaveVersion(oldVersion);
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
            var lstArgs = GetAllArguments(extraArguments);
            foreach (var arg in lstArgs) psi.ArgumentList.Add(arg);

            // start new process
            Console.WriteLine($"[START NEW PROCESS] {psi.FileName}");

            var process = Process.Start(psi);
            process.WaitForExit();

            return process.ExitCode;
        }


        public async Task<bool> PublishAvailableAsync(Func<Version, Version, bool> publishMessageContinue=null)
        {
            var localVersion = await GetLocalVersion();
            var remoteVersion = await GetRemoteVersion();

            // Console.WriteLine($"[local version:{localVersion} <-> remote version:{remoteVersion}]");

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
                Console.WriteLine($"[ERROR] Get Zip from {exePath} is nulll");
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

        public static List<string> GetAllArguments(Func<List<string>> extraArguments = null)
        {
            //starts the (hopefully correcly updated) process using the original executable name startup arguments.
            var arguments = Environment.GetCommandLineArgs();

            //1st argument is always the executable path (see AppCore from MSDN  reference).
            var args = new List<string>();
            for (int i = 1; i < arguments.Length; i++)
            {
                args.Add(arguments[i]);
            }

            var extraArgs = extraArguments?.Invoke();
            if (extraArgs != null)
            {
                //keep it clean.
                foreach (var extraArg in extraArgs)
                {
                    if (!args.Contains(extraArg)) args.Add(extraArg);
                }
            }

            return args;
        }

    }

}
