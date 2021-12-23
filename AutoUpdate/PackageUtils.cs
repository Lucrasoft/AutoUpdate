using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using AutoUpdate.Models;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Globalization;
using System.Diagnostics;

namespace AutoUpdate
{
    public class PackageUtils
    {
        public static async Task PostMemoryStreamToDownloadUrlAsync(MemoryStream data, string filename, Uri url, EventHandler<ProgressUploadEvent> handler, string operationText = "uploading")
        {
            filename = Path.GetFileName(filename);
            var name = Path.GetFileNameWithoutExtension(filename);

            using var client = new HttpClient();
            using var content = new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture))
            {
                { new StreamContent(data), name, filename }
            };

            using var message = await client.PostAsync(url, content);
            var input = await message.Content.ReadAsStringAsync();
        }

        public static async Task<MemoryStream> GetMemoryStreamForDownloadUrlAsync(Uri url, EventHandler<ProgressDownloadEvent> handler, string operationText = "downloading")
        {

            using (HttpResponseMessage response = await Updater.HTTPClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                long streamLength = -1;
                if (response.Content.Headers.ContentLength != null) streamLength = (long)response.Content.Headers.ContentLength;

                using (Stream remoteStream = await response.Content.ReadAsStreamAsync())
                {
                    return FillFromRemoteStream(remoteStream, streamLength, handler, operationText);
                }
            }
        }

        public static MemoryStream FillFromRemoteStream(Stream remoteStream)
        {
            using MemoryStream ms = new();
            remoteStream.CopyTo(ms);
            return ms;
        }

        public static MemoryStream FillFromRemoteStream(Stream remoteStream, long streamLength, EventHandler<ProgressDownloadEvent> handler, string operationText="downloading")
        {
            var returnStream = new MemoryStream();

            //buffer, download, calculate percentage.
            int lastPercentage = -1;
            if (streamLength > 0) handler?.Invoke(handler.Target, new(operationText, lastPercentage));

            byte[] buffer = new byte[65536]; // read in chunks of 64KB
            int bytesRead;
            while ((bytesRead = remoteStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                //do something with data in buffer, up to the size indicated by bytesRead
                returnStream.Write(buffer, 0, bytesRead);

                if (handler != null && streamLength > 0)
                {
                    int completePercentage = (int)Math.Round(((double)returnStream.Length * 100) / streamLength, 0, MidpointRounding.AwayFromZero);
                    if (completePercentage != lastPercentage)
                    {
                        lastPercentage = completePercentage;
                        handler(handler.Target, new(operationText, completePercentage));
                    }
                }
            }

            return returnStream;
        }

        public static void ExtractArchive(ZipArchive archive, string installationPath, EventHandler<ProgressDownloadEvent> handler, string operationText="extracting")
        {
            int archiveEntryCount = archive.Entries.Count;
            int unpackedEntryCount = 0;
            int lastPercentage = -1;

            if(handler != null)
            {
                handler.Invoke(handler.Target, new(operationText, lastPercentage));
            }

            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                // Get Filename
                string destinationPath = Path.GetFullPath(Path.Combine(installationPath, entry.FullName));
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)); // Create directory if it doesn't already exist

                try
                {
                    if (string.IsNullOrWhiteSpace(entry.Name)) continue; // Directory only!

                    // Try to overwrite the file
                    entry.ExtractToFile(destinationPath, true);
                }
                catch (IOException)
                {
                    // Hm.. It is probably locked. So we rename this file to .old and try again!
                    string oldFilename = destinationPath + ".old";
                    if (File.Exists(oldFilename)) { File.Delete(oldFilename); }
                    File.Move(destinationPath, destinationPath + ".old");

                    // try again!
                    entry.ExtractToFile(destinationPath, true);
                }
                finally
                {
                    unpackedEntryCount += 1;

                    if(handler != null)
                    {
                        int completePercentage = (int)Math.Round(((double)unpackedEntryCount * 100) / archiveEntryCount, 0, MidpointRounding.AwayFromZero);
                        if (completePercentage != lastPercentage)
                        {
                            lastPercentage = completePercentage;
                            handler.Invoke(handler.Target, new(operationText, completePercentage));
                        }
                    }

                }

            }

        }

        public static Stream GenerateStream(object obj)
        {
            var s = JsonConvert.SerializeObject(obj);

            using var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static string GetVersionString(FileVersionInfo info) => GetVersionString(new Version(info.FileVersion));

        public static string GetVersionString(Version version)
        {
            var major = $"{Math.Max(version.Major, 0)}";
            var minor = $"{Math.Max(version.Minor, 0)}";
            var build = $"{Math.Max(version.Build, 0)}";
            var revision = $"{Math.Max(version.Revision, 0)}";

            return $"{major}.{minor}.{build}.{revision}";
        }

    }
}
