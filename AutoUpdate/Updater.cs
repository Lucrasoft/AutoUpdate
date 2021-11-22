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
        public static HttpClient HTTPClient = new();
        public static PackageUpdateEnum PackageUpdateType = PackageUpdateEnum.InPlace;

        public event EventHandler<ProgressDownloadEvent> OnDownloadProgress;

        private readonly IPackage package;
        private readonly PackageHelper packageHelper;

        //TODO include logger from DI
        //private readonly ILogger<Updater> logger;

        public Updater(IVersionProvider local, IVersionProvider remote, IPackage package, PackageUpdateEnum type, HttpClient client)
        {
            this.package = package;

            packageHelper = new(local, remote);
            PackageUpdateType = type;
            HTTPClient = client;
        }

        public async Task<bool> UpdateAvailableAsync(Func<Version, Version, bool> updateMessageContinue=null)
        {
            var localVersion = await GetLocalVersion();
            var remoteVersion = await GetRemoteVersion();

            // catch failed remote version
            if(packageHelper.VersionIsFailing(remoteVersion))
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

        public async Task<bool> Update(EventHandler<ProgressDownloadEvent> onDownloadProgress=null)
        {
            OnDownloadProgress = onDownloadProgress;
            try { OnDownloadProgress?.Invoke(this, new("downloading", -1)); }
            catch (Exception) { }

            var remoteVersion = await GetRemoteVersion();
            var package = await this.package.GetContentAsync(remoteVersion, OnDownloadProgress);

            // set new version
            var success = packageHelper.SetVersion(package, remoteVersion, OnDownloadProgress);
            if(!success)
            {
                Console.WriteLine("[ERROR] remote version do not match with .EXE version!");
            }

            return success;
        }

        public bool Restart(Func<List<string>> extraArguments=null)
        {
            //starts the (hopefully correcly updated) process using the original executable name startup arguments.
            var arguments = Environment.GetCommandLineArgs();
            var exeFile = Process.GetCurrentProcess().MainModule.FileName;
            var exePath = Path.GetDirectoryName(exeFile);
            var lstArgs = new List<string>();

            // Run: (pre/post)-install.* (.bat / .cmd /.ps /.exe) bestanden
            packageHelper.RunPreAndPostInstall();

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


            // In-Place (child process)
            var psi = new ProcessStartInfo
            {
                FileName = exeFile,
                WorkingDirectory = exePath,
            };

            // add arguments
            foreach (var arg in lstArgs)
            {
                psi.ArgumentList.Add(arg);
            }

            // start new process
            Process process = new();
            process.StartInfo = psi;
            if (!process.Start())
            {
                Console.WriteLine($"[RESTART FAILED] {psi.FileName}");
                return false;
            }
            
            Console.WriteLine($"[RESTART] {psi.FileName}");
            return true;
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
            var localVersion = await GetLocalVersion();
            var exeFile = Process.GetCurrentProcess().MainModule.FileName;
            var exePath = Path.GetDirectoryName(exeFile);
            var currVersion = PackageHelper.CurrentVersionToZip(exePath);

            await package.SetContentAsync(currVersion, localVersion, onUploadProgress);
            await packageHelper.SetRemoteVersionAsync(localVersion);
        }

        public async Task<Version> GetLocalVersion() => await packageHelper.GetLocalVersionAsync();

        public async Task<Version> GetRemoteVersion() => await packageHelper.GetRemoteVersionAsync();

    }

}
