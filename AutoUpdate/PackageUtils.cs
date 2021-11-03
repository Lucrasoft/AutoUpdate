using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.IO.Compression;

namespace AutoUpdate
{
    public class PackageUtils
    {
        public static async Task<MemoryStream> GetMemoryStreamForDownloadUrl(HttpClient client, Uri url, Action<string, int> currentOperationTotalPercentDone, string operationText = "downloading")
        {

            using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                long streamLength = -1;
                if (response.Content.Headers.ContentLength != null) streamLength = (long)response.Content.Headers.ContentLength;

                using (Stream remoteStream = await response.Content.ReadAsStreamAsync())
                {
                    return FillFromRemoteStream(remoteStream, streamLength, currentOperationTotalPercentDone, operationText);
                }
            }
        }

        public static MemoryStream FillFromRemoteStream(Stream remoteStream, long streamLength, Action<string, int> currentOperationTotalPercentDone, string operationText)
        {
            MemoryStream returnStream = new MemoryStream();

            //buffer, download, calculate percentage.
            int lastPercentage = -1;
            if (streamLength > 0) currentOperationTotalPercentDone?.Invoke(operationText, lastPercentage);

            byte[] buffer = new byte[65536]; // read in chunks of 64KB
            int bytesRead;
            while ((bytesRead = remoteStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                //do something with data in buffer, up to the size indicated by bytesRead
                returnStream.Write(buffer, 0, bytesRead);

                if (currentOperationTotalPercentDone != null && streamLength > 0)
                {
                    int completePercentage = (int)Math.Round(((double)returnStream.Length * 100) / streamLength, 0, MidpointRounding.AwayFromZero);
                    if (completePercentage != lastPercentage)
                    {
                        lastPercentage = completePercentage;
                        currentOperationTotalPercentDone(operationText, completePercentage);
                    }
                    
                }
            }
            return returnStream;
        }

        public static void ExtractArchive(ZipArchive archive, string installationPath, Action<string, int> currentOperationTotalPercentDone, string operationText = "extracting")
        {
            int archiveEntryCount = archive.Entries.Count;
            int unpackedEntryCount = 0;
            int lastPercentage = -1;
            currentOperationTotalPercentDone?.Invoke(operationText, lastPercentage);

            foreach (ZipArchiveEntry entry in archive.Entries)
            {
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
                    //Hm.. It is probably locked. So we rename this file to .old and try again!
                    string oldFilename = destinationPath + ".old";
                    if (File.Exists(oldFilename)) { File.Delete(oldFilename); }
                    File.Move(destinationPath, destinationPath + ".old");

                    //try again!
                    entry.ExtractToFile(destinationPath, true);
                }
                finally
                {
                    unpackedEntryCount += 1;
                    if (currentOperationTotalPercentDone != null)
                    {
                        int completePercentage = (int)Math.Round(((double)unpackedEntryCount * 100) / archiveEntryCount, 0, MidpointRounding.AwayFromZero);
                        if (completePercentage != lastPercentage)
                        {
                            lastPercentage = completePercentage;
                            currentOperationTotalPercentDone(operationText, completePercentage);
                        }
                    }
                }
            }
        }
    }
}
