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

namespace AutoUpdate
{
    internal class Updater : IUpdater
    {
        private readonly IVersionProvider localProvider;
        private readonly IVersionProvider remoteProvider;
        private readonly IPackage package;

        //TODO include logger from DI
        private readonly ILogger<Updater> logger;


        public event EventHandler<DownloadProgressEventArgs> OnDownloadProgress;


        public Updater(IVersionProvider local, IVersionProvider remote, IPackage package)
        {
            this.localProvider = local;
            this.remoteProvider = remote;
            this.package = package;
        }

        public async Task<bool> UpdateAvailableAsync()
        {
            return await UpdateAvailableAsync(null);
        }

        public async Task<bool> UpdateAvailableAsync(Func<Version, Version, bool> updateMessageContinue)
        {
            var localVersion = await localProvider.GetVersionAsync();
            var remoteVersion = await remoteProvider.GetVersionAsync();
            if (remoteVersion > localVersion)
            {
                bool continueUpdate = true;
                if (updateMessageContinue != null) continueUpdate = updateMessageContinue.Invoke(localVersion, remoteVersion);
                return continueUpdate;
            }
            return false;
        }


        public async Task Update()
        {
            try { OnDownloadProgress?.Invoke(this, new("downloading", -1)); }
            catch (Exception) { }

            var remoteVersion = await remoteProvider.GetVersionAsync();
            var package = await this.package.GetContentAsync(remoteVersion, OnDownloadProgress);

            //SKIP : unpack package in temp location is NOT necessaary, we can extract from memory! 
            //var path = System.IO.Path.GetTempPath();
            //var unpackFolder = System.IO.Path.Combine(path, Guid.NewGuid().ToString());

            //TODO : voor nu doen we INPLACE ipv SIDE bY SIDE.

            //Huidige locatie van de .exe file bepaald de bestemming
            var exeFile = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            var exePath = Path.GetDirectoryName(exeFile);
            var archive = new ZipArchive(new MemoryStream(package));

            PackageUtils.ExtractArchive(archive, exePath, OnDownloadProgress);
        }

        public async Task Update(EventHandler<DownloadProgressEventArgs> onDownloadProgress)
        {
            OnDownloadProgress = onDownloadProgress;
            await Update();
        }


        public void Restart()
        {
            Restart(null);
        }

        //3b) het restart proces moet goed getest worden.. Het is niet heel eenvoudig, 
        //    want je moet het huidige proces afbreken + nieuwe starten, en alle commandline arguments moeten doorgeven worden , 
        //    maar vooral moet ook de std in / std out goed blijven werken. Dat moet even goed getest worden.
        public void Restart(Func<List<string>> extraArguments)
        {
            //TODO : loop prevention!! but how?!
            // -> what loop? when a new executable is introduced with an update? or..?

            //starts the (hopefully correcly updated) process using the original executable name startup arguments.
            var arguments = Environment.GetCommandLineArgs();
            var lstArgs = new List<string>();
            for (int i = 1; i < arguments.Length; i++) //1st argument is always the executable path (see AppCore from MSDN reference).
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

            // most relaibly way to determine the running .exe is -> 
            var exeFile = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            // working dir could be <> executable dir.
            var curPath = Directory.GetCurrentDirectory();
                        
            var psi = new ProcessStartInfo();
            psi.FileName = exeFile;
            psi.WorkingDirectory = curPath;

            //add arguments. With new ArgumentList.Add() methed we need not be concerned with adding quotes around white-spaced arguments etc. etc.
            foreach (var arg in lstArgs)
            {
                psi.ArgumentList.Add(arg);
            }
            
            Process.Start(psi);
            //no do NOT exit here, this is the callers' responsibility (e.g. bootloader needs to restore command-line..).
            //Environment.Exit(0);  
        }


        public void Publish()
        {
            //generate zip archive from current location...?

            //Huidige locatie van de .exe file bepaald de bestemming
            var exeFile = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            var exePath = Path.GetDirectoryName(exeFile);

            ZipFile.CreateFromDirectory(exePath, "c:\\temp\\test.zip");

            //var zipStream = new MemoryStream();
            //var archive = new ZipArchive(zipStream, ZipArchiveMode.Create);
           
            //foreach (var filename in Directory.GetFiles(exePath, "*", SearchOption.AllDirectories))
            //{
            //    archive.CreateEntryFromFile(filename, filename);
            //}
        }

        public Task<Version> GetLocalVersion()
        {
            return localProvider.GetVersionAsync();   
        }

        public Task<Version> GetRemoteVersion()
        {
            return remoteProvider.GetVersionAsync();
        }

    }


}
