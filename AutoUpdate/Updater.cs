using AutoUpdate.Package;
using System;
using System.Threading.Tasks;
using System.Runtime;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.IO.Compression;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using AutoUpdate.Models;
using System.Text.Json;
using System.Linq;
using Microsoft.VisualStudio.Utilities;

namespace AutoUpdate
{
    internal class Updater : IUpdater
    {
        private readonly IVersionProvider localProvider;
        private readonly IVersionProvider remoteProvider;
        private readonly IPackage package;

        private readonly string exeFile;
        private readonly string exePath;
        private readonly FolderData folderData;

        //TODO include logger from DI
        private readonly ILogger<Updater> logger;

        public event EventHandler<DownloadProgressEventArgs> OnDownloadProgress;

        public Updater(IVersionProvider local, IVersionProvider remote, IPackage package)
        {
            this.localProvider = local;
            this.remoteProvider = remote;
            this.package = package;

            //Huidige locatie van de .exe file bepaald de bestemming
            exeFile = Process.GetCurrentProcess().MainModule.FileName;
            exePath = Path.GetDirectoryName(exeFile);
            folderData = JsonHelper.Read<FolderData>(path: exePath);

            // remove duplicated files
            RemoveDuplicatedFiles();
        }

        private void RemoveDuplicatedFiles()
        {
            var files = Directory.GetFiles(exePath);
            var currFileNames = folderData.CurrentFileNames;

            // update filenames
            if (currFileNames.Count > 0 && !currFileNames.SequenceEqual(files))
            {
                var duplicated = files.Except(currFileNames).ToList();
                if (duplicated.Count > 0)
                {
                    // kill previous running program file
                    KillProcesses(duplicated);

                    foreach (var filename in duplicated)
                    {
                        try
                        {
                            File.Delete(filename);
                            Console.WriteLine($"[DELETE] {filename}");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"[DELETE FAILED] {filename}\n\t --> Exception Message: {e.Message}");
                        }
                    }
                }
            }
        }

        private static void KillProcesses(List<string> files)
        {
            // kill previous running program file
            var exeFiles = files.Where(a => a.EndsWith(".exe"));
            foreach (var exe in exeFiles)
            {
                foreach (var proc in Process.GetProcessesByName(exe))
                {
                    proc.Kill();
                }
            }
        }

        public async Task<bool> UpdateAvailableAsync() => await UpdateAvailableAsync(null);
        public async Task<bool> UpdateAvailableAsync(Func<Version, Version, bool> updateMessageContinue)
        {
            var localVersion = await GetLocalVersion();
            var remoteVersion = await GetRemoteVersion();

            Console.WriteLine(
                $"[INFO version]\n\tREMOTE:{remoteVersion}\n\tLOCAL:{localVersion}"
            );

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

        public async Task Update() => await Update(null);
        public async Task Update(EventHandler<DownloadProgressEventArgs> onDownloadProgress)
        {
            OnDownloadProgress = onDownloadProgress;
            try { OnDownloadProgress?.Invoke(this, new("downloading", -1)); }
            catch (Exception) { }

            var remoteVersion = await remoteProvider.GetVersionAsync();
            var package = await this.package.GetContentAsync(remoteVersion, OnDownloadProgress);

            //SKIP : unpack package in temp location is NOT necessaary, we can extract from memory! 
            //var path = System.IO.Path.GetTempPath();
            //var unpackFolder = System.IO.Path.Combine(path, Guid.NewGuid().ToString());

            //TODO : voor nu doen we INPLACE ipv SIDE bY SIDE.

            //Huidige locatie van de .exe file bepaald de bestemming
            var archive = new ZipArchive(new MemoryStream(package));

            // set folder names and current names
            UpdateFolderFileNames(archive);

            PackageUtils.ExtractArchive(archive, exePath, OnDownloadProgress);
        }

        private void UpdateFolderFileNames(ZipArchive archive)
        {
            var prevFileNames = Directory.GetFiles(exePath).ToList();
            var currFileNames = archive.Entries.Select(a => $"{exePath}\\{a.FullName}").ToList();

            // set all history filenames
            (_, var jsonFile) = JsonHelper.GetFile<FolderData>(path:exePath);
            currFileNames.Add(jsonFile);
            prevFileNames.Add(jsonFile);

            folderData.CurrentFileNames = currFileNames.ToList();
            folderData.PreviousFileNames = prevFileNames.ToList();
            JsonHelper.Write(folderData, path: exePath);
        }

        public void Restart() => Restart(null);
        public void Restart(Func<List<string>> extraArguments)
        {
            //starts the (hopefully correcly updated) process using the original executable name startup arguments.
            var arguments = Environment.GetCommandLineArgs();
            var lstArgs = new List<string>();

            //1st argument is always the executable path (see AppCore from MSDN  reference).
            for (int i = 1; i < arguments.Length; i++) 
            {
                lstArgs.Add(arguments[i]);
            }

            var extraArgs = extraArguments?.Invoke();
            if (extraArgs != null)
            {
                //keep it clean.
                foreach (var extraArg in extraArgs)
                {
                    if (!lstArgs.Contains(extraArg)) lstArgs.Add(extraArg);
                }
            }

            // restart EXE
            var file = GetExecutableFilename();
            var path = Path.GetDirectoryName(file);
            var psi = new ProcessStartInfo
            {
                FileName = file,
                WorkingDirectory = path,
                WindowStyle = ProcessWindowStyle.Normal,
                CreateNoWindow = false,
                UseShellExecute = true,
                
            };

            //add arguments. With new ArgumentList.Add() methed we need not be concerned with adding quotes around white-spaced arguments etc. etc.
            foreach (var arg in lstArgs)
            {
                psi.ArgumentList.Add(arg);
            }

            Console.WriteLine($"[RESTART] {psi.FileName}");


            //TODO: There is a change that the .exe filename changed. so old data has to been removed.
            // How to do this????

            Process.Start(psi);
            //Process.GetCurrentProcess().Kill();

            //no do NOT exit here, this is the callers' responsibility (e.g. bootloader needs to restore command-line..).
            //Environment.Exit(0);
        }

        private string GetExecutableFilename()
        {
            var files = folderData.CurrentFileNames;
            var exes = files.Where(a => a.EndsWith(".exe")).ToList();

            if (exes.Count > 1 || !exes.Any())
            {
                var docs = files.Except(folderData.PreviousFileNames).ToList();
                exes = docs.Where(a => a.EndsWith(".exe")).ToList();
                var file = exes.Count == 0 ? exeFile : exes.First();

                Console.WriteLine(
                    $"(AutoUpdate::Updater::GetExecutableFilename)\n" +
                    $"[WARNING] There are more .exe Files, restart on {file}\n"
                );

                return exeFile;
            }

            return exes.First();
        }

        public void Publish()
        {
            //generate zip archive from current location...?

            ZipFile.CreateFromDirectory(exePath, "c:\\temp\\test.zip");

            //var zipStream = new MemoryStream();
            //var archive = new ZipArchive(zipStream, ZipArchiveMode.Create);
           
            //foreach (var filename in Directory.GetFiles(exePath, "*", SearchOption.AllDirectories))
            //{
            //    archive.CreateEntryFromFile(filename, filename);
            //}
        }

        public async Task<Version> GetLocalVersion() => await localProvider.GetVersionAsync();

        public async Task<Version> GetRemoteVersion() => await remoteProvider.GetVersionAsync();
    }


}
