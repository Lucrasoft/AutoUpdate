using AutoUpdate.Models;
using AutoUpdate.Provider;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace AutoUpdate.Package
{
    public class PackageHelper
    {
        public readonly IVersionProvider LocalVersion;
        public readonly IVersionProvider RemoteVersion;

        public PackageVersionsObject Versions { get; private set; }
        public string FileName { get; private set; }
        public string Path { get; private set; }

        public PackageHelper(IVersionProvider local, IVersionProvider remote, string filename ="./package_versions.json")
        {
            LocalVersion = local;
            RemoteVersion = remote;

            if (filename.StartsWith("./"))
            {
                filename = filename.Replace("./", "");
                var exe = Process.GetCurrentProcess().MainModule.FileName;

                Path = System.IO.Path.GetDirectoryName(exe);
                FileName = $"{Path}/{filename}";
            }
            else
            {
                Path = System.IO.Path.GetDirectoryName(filename);
                FileName = filename;
            }

            Versions = JsonHelper.Read<PackageVersionsObject>(FileName);
        }


        // Package

        public static byte[] CurrentVersionToZip(string folderName)
        {
            // get folder
            var allFileNames = Directory.GetFiles(folderName);
            var allFiles = new Dictionary<string, byte[]>();

            foreach (var filename in allFileNames)
            {
                var data = File.ReadAllBytes(filename);
                allFiles.Add(filename, data);
            }

            // compress folder
            byte[] compressedBytes;
            using (var outStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(outStream, ZipArchiveMode.Create, true))
                {
                    foreach (var file in allFiles)
                    {
                        var fileInArchive = archive.CreateEntry(file.Key, CompressionLevel.Optimal);

                        using var entryStream = fileInArchive.Open();
                        using var fileToCompressStream = new MemoryStream(file.Value);

                        fileToCompressStream.CopyTo(entryStream);
                    }
                }
                compressedBytes = outStream.ToArray();
            }

            return compressedBytes;
        }

        public bool SetVersion(string folderName, Version version, EventHandler<ProgressDownloadEvent> onDownloadProgress = null)
        {
            var package = CurrentVersionToZip(folderName);
            return SetVersion(package, version, onDownloadProgress);
        }

        public bool SetVersion(byte[] package, Version version, EventHandler<ProgressDownloadEvent> onDownloadProgress = null)
        {
            var exe = Process.GetCurrentProcess().MainModule.FileName;
            var exename = System.IO.Path.GetFileName(exe);
            var remoteExe = $"{Path}\\RemoteVersion.exe";

            // save remote version EXE
            // TODO: Set into Memory. (Now we create a file)
            var archive = new ZipArchive(new MemoryStream(package));
            foreach (ZipArchiveEntry entry in archive.Entries)
                if (entry.Name == exename)
                    entry.ExtractToFile(remoteExe, true);

            // check zip version == exe version
            var versionInfo = FileVersionInfo.GetVersionInfo(remoteExe);
            var newVersion = PackageUtils.GetVersionString(versionInfo);
            var oldVersion = PackageUtils.GetVersionString(version);

            // remove local `RemoteVersion.exe` file
            if(File.Exists(remoteExe))
            {
                File.Delete(remoteExe);
            }


            // return version
            if (oldVersion != newVersion)
            {
                SaveFailedVersion(oldVersion);
                return false;
            }

            // in-place
            PackageUtils.ExtractArchive(archive, Path, onDownloadProgress);

            SaveVersion(oldVersion);
            return true;
        }

        private void SaveVersion(string version)
        {
            Versions.Versions.Add(version);
            JsonHelper.Write(Versions, FileName);
        }

        private void SaveFailedVersion(string version)
        {
            Versions.FailedVersions.Add(version);
            JsonHelper.Write(Versions, FileName);
        }

        public bool VersionIsFailing(Version version)
        {
            string vname = PackageUtils.GetVersionString(version);
            return Versions.FailedVersions.Contains(vname);
        }

        public void RunPreAndPostInstall()
        {
            foreach (var name in Directory.GetFiles(Path))
            {
                var filename = System.IO.Path.GetFileNameWithoutExtension(name).ToLower();
                var ext = System.IO.Path.GetExtension(filename).ToLower();

                var type = "";
                var matchExt = ext.Contains("ps") || ext.Contains("bat") || ext.Contains("cmd") || ext.Contains("exe");
                if (filename == "pre-install") type = "PRE";
                else if (filename == "post-install") type = "POST";

                // skip invalid filenames
                if (type.Length == 0 || !matchExt) continue;

                // execute
                Console.WriteLine($"[Run {type}-INSTALL] {filename}");
                if (ext.Contains("ps"))
                {
                    // powershell script on command prompt (incl. bypass execution-policy)
                    filename = $"PowerShell.exe -command \"cd {Path}; Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass; {filename}\"";
                }

                ExecuteCommand(filename);
            }
        }

        private static void ExecuteCommand(string command)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            var process = Process.Start(processInfo);
            process.WaitForExit();

            // *** Read the streams ***
            // Warning: This approach can lead to deadlocks, see Edit #2
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            var exitCode = process.ExitCode;

            Console.WriteLine("output >> " + (String.IsNullOrEmpty(output) ? "(none)" : output));
            Console.WriteLine("error  >> " + (String.IsNullOrEmpty(error) ? "(none)" : error));
            Console.WriteLine("ExitCode: " + exitCode.ToString(), "ExecuteCommand");
            process.Close();
        }



        // Version

        public async Task<Version> GetLocalVersionAsync() => await LocalVersion.GetVersionAsync();

        public async Task SetRemoteVersionAsync(Version version) => await RemoteVersion.SetVersionAsync(version);

        public async Task<Version> GetRemoteVersionAsync() => await RemoteVersion.GetVersionAsync();

    }
}
