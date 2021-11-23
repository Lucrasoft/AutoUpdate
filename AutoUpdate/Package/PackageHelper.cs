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

        public static Stream CompressStreams(IList<Stream> Streams, IList<string> StreamNames, Stream OutputStream = null)
        {
            var Response = new MemoryStream();

            using (var ZippedFile = new ZipArchive(Response, ZipArchiveMode.Create, true))
            {
                for (int i = 0, length = Streams.Count; i < length; i++)
                    using (var entry = ZippedFile.CreateEntry(StreamNames[i]).Open())
                    {
                        Streams[i].CopyTo(entry);
                    }

            }
            if (OutputStream != null)
            {
                Response.Seek(0, SeekOrigin.Begin);
                Response.CopyTo(OutputStream);
            }

            return Response;
        }

        public static async Task<byte[]> CurrentVersionToZip(string folderName)
        {
            // TODO: Set generated zip into Memory.
            var zipName = $"{folderName}/../CurrentVersionToZip.zip";

            // create file
            ZipFile.CreateFromDirectory(folderName, zipName);

            // read file
            var bytes = await File.ReadAllBytesAsync(zipName);
            
            // delete file
            File.Delete(zipName);

            return bytes;

            ////zip content
            //using (var ms = new MemoryStream())
            //{
            //    using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
            //    {
            //        foreach (var file in files)
            //        {
            //            ZipArchiveEntry orderEntry = archive.CreateEntry(file.Key); //create a file with this name
            //            using var writer = new BinaryWriter(orderEntry.Open());
            //            writer.Write(file.Value); //write the binary data
            //        }
            //    }

            //    //ZipArchive must be disposed before the MemoryStream has data
            //    return ms.ToArray();
            //}

            //return null;

            //// compress folder
            //byte[] compressedBytes;
            //using (var outStream = new MemoryStream())
            //{
            //    using (var archive = new ZipArchive(outStream, ZipArchiveMode.Create, true))
            //    {
            //        var fileInArchive = archive.CreateEntry(rootFolder, CompressionLevel.Optimal);

            //        using var entryStream = fileInArchive.Open();
            //        using var fileToCompressStream = new MemoryStream(file.Value);

            //        fileToCompressStream.CopyTo(entryStream);


            //        //foreach (var file in files)
            //        //{
            //        //    var entryName = file.Key.Replace("./", "").Replace(".\\", "").Replace("\\", "/");
            //        //    var fileInArchive = archive.CreateEntry(entryName, CompressionLevel.Optimal);

            //        //    using var entryStream = fileInArchive.Open();
            //        //    using var fileToCompressStream = new MemoryStream(file.Value);

            //        //    fileToCompressStream.CopyTo(entryStream);
            //        //}
            //    }
            //    compressedBytes = outStream.ToArray();
            //}

            //return compressedBytes;
        }

        public async Task<bool> SetVersion(string folderName, Version version, EventHandler<ProgressDownloadEvent> onDownloadProgress = null)
        {
            var package = await CurrentVersionToZip(folderName);
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
            if(File.Exists(remoteExe)) File.Delete(remoteExe);


            // return version
            if (oldVersion != newVersion)
            {
                SaveFailedVersion(oldVersion);
                return false;
            }

            // set files
            switch (Updater.PackageUpdateType)
            {
                case PackageUpdateEnum.SideBySide:
                    Path = $"{Path}/../{newVersion}";
                    break;

                //default:
                //case PackageUpdateEnum.InPlace:
                //    Path = $"{Path}";
                //    break;
            }

            // save path
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
