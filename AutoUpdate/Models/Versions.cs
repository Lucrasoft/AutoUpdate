using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace AutoUpdate.Models
{
    public class Versions
    {
        public readonly IVersionProvider LocalProvider;
        public readonly IVersionProvider RemoteProvider;

        public VersionsData Data { get; private set; }
        public string FileName { get; private set; }
        public string Path { get; private set; }

        public Versions(IVersionProvider local, IVersionProvider remote, string filename ="./folder_versions.json")
        {
            LocalProvider = local;
            RemoteProvider = remote;

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

            Data = JsonHelper.Read<VersionsData>(FileName);
        }

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
            var newVersion = GetVersionString(versionInfo);
            var oldVersion = GetVersionString(version);

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
            Data.Versions.Add(version);
            JsonHelper.Write(Data, FileName);
        }

        private void SaveFailedVersion(string version)
        {
            Data.FailedVersions.Add(version);
            JsonHelper.Write(Data, FileName);
        }

        public bool IsFailedVersion(Version version)
        {
            string vname = GetVersionString(version);
            return Data.FailedVersions.Contains(vname);
        }

        public async Task<Version> GetLocalVersionAsync() => await LocalProvider.GetVersionAsync();

        public async Task<Version> GetRemoteVersionAsync() => await RemoteProvider.GetVersionAsync();

        public static string GetVersionString(FileVersionInfo info)
        {
            return GetVersionString(new Version(info.FileVersion));
        }

        public static string GetVersionString(Version version)
        {
            return $"{Math.Max(version.Major, 0)}.{Math.Max(version.Minor, 0)}.{Math.Max(version.Build, 0)}.{Math.Max(version.Revision, 0)}";
        }

    }
}
