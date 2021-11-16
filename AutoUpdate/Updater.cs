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

namespace AutoUpdate
{
    internal class Updater : IUpdater
    {
        private readonly IVersionProvider localProvider;
        private readonly IVersionProvider remoteProvider;
        private readonly IPackage package;

        private readonly string exeFile;
        private readonly string exePath;
        private readonly string jsonFilename;
        private List<string> currFilenames = new();
        private List<string> prevFilenames = new();

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
            jsonFilename = $"{exePath}\\prev_filenames.json";

            RemoveDuplicatedFiles();
        }

        private void RemoveDuplicatedFiles()
        {
            // get filenames
            var files = Directory.GetFiles(exePath);
            currFilenames = new List<string>(files);

            if (File.Exists(jsonFilename))
            {
                var text = File.ReadAllText(jsonFilename);
                prevFilenames = JsonSerializer.Deserialize<List<string>>(text);
            }

            // update filenames
            if (prevFilenames.Count != currFilenames.Count)
            {
                if(prevFilenames.Count > 0)
                {
                    var duplicates = currFilenames.Except(prevFilenames).ToList();
                    foreach (var filename in duplicates)
                    {
                        // skip 
                        if (filename == jsonFilename) continue;

                        File.Delete(filename);
                        currFilenames.Remove(filename);
                    }

                    Console.WriteLine(
                         $"\n(AutoUpdate::Updater::RemoveDuplicatedFiles())\n" +
                         $"[INFO] Removed: [\n\t{string.Join(",\n\t ", duplicates)}\n]"
                    );
                }

                string json = JsonSerializer.Serialize(currFilenames);
                File.WriteAllText(jsonFilename, json);
            }
        }


        public async Task<bool> UpdateAvailableAsync() => await UpdateAvailableAsync(null);
        public async Task<bool> UpdateAvailableAsync(Func<Version, Version, bool> updateMessageContinue)
        {
            var localVersion = await GetLocalVersion();
            var remoteVersion = await GetRemoteVersion();

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

            PackageUtils.ExtractArchive(archive, exePath, OnDownloadProgress);
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

            #region restart EXE
            var file = GetExecutableFilename();
            var path = Path.GetDirectoryName(file);
            var psi = new ProcessStartInfo
            {
                FileName = file,
                WorkingDirectory = path
            };

            //add arguments. With new ArgumentList.Add() methed we need not be concerned with adding quotes around white-spaced arguments etc. etc.
            foreach (var arg in lstArgs)
            {
                psi.ArgumentList.Add(arg);
            }
            
            Process.Start(psi);
            #endregion

            //no do NOT exit here, this is the callers' responsibility (e.g. bootloader needs to restore command-line..).
            //Environment.Exit(0);
        }

        private string GetExecutableFilename()
        {
            var exes = Directory.GetFiles(exePath).Where(a => a.EndsWith(".exe")).ToList();

            if (exes.Count > 1 || !exes.Any())
            {
                var files = currFilenames.Except(prevFilenames).ToList();
                exes = files.Where(a => a.EndsWith(".exe")).ToList();
                var file = exes.Count == 0 ? exeFile : exes.First();

                Console.WriteLine(
                    $"(AutoUpdate::Updater::Restart(Func<List<string>>))\n" +
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
