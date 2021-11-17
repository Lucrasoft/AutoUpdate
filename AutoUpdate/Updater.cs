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
        private readonly IExeFolder exeFolder;

        public event EventHandler<DownloadProgressEventArgs> OnDownloadProgress;

        //TODO include logger from DI
        //private readonly ILogger<Updater> logger;


        public Updater(IVersionProvider local, IVersionProvider remote, IPackage package)
        {
            this.localProvider = local;
            this.remoteProvider = remote;
            this.package = package;
            this.exeFolder = new ExeFolder();

            // remove duplicated files
            exeFolder.RemoveDuplicatedFileNames();
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
            exeFolder.UpdateFileNames(archive);

            PackageUtils.ExtractArchive(archive, exeFolder.ExePath, OnDownloadProgress);
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

            // Start new EXE
            var file = exeFolder.GetExecutableFileName();
            var path = Path.GetDirectoryName(file);
            var psi = new ProcessStartInfo
            {
                FileName = @"cmd",
                Arguments = $"/C start {file} {string.Join(" ", lstArgs)}",
                WorkingDirectory = path,
                WindowStyle = ProcessWindowStyle.Normal
            };

            Console.WriteLine($"[RESTART] {psi.Arguments}");

            Process.Start(psi);

            //no do NOT exit here, this is the callers' responsibility (e.g. bootloader needs to restore command-line..).
            //Environment.Exit(0);
            Process.GetCurrentProcess().Kill();
        }

        public void Publish()
        {
            //generate zip archive from current location...?

            ZipFile.CreateFromDirectory(exeFolder.ExePath, "c:\\temp\\test.zip");

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
