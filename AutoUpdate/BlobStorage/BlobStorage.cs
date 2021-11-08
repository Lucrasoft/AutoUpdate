using AutoUpdate.Providers;
using Azure.Storage.Blobs;
using Newtonsoft.Json;
using Sample;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate
{
    public class BlobStorage
    {
        private readonly string versionFilename = "version.json";
        
        private readonly BlobServiceClient blobServiceClient;
        private readonly BlobContainerClient containerClient;

        private Version LocalVersion { get; set; }

        private VersionFile RemoteVersion { get; set; }

        private Stream Stream { get; set; }

        public BlobStorage(string connectionString, string container)
        {
            blobServiceClient = new BlobServiceClient(connectionString);
            containerClient = blobServiceClient.GetBlobContainerClient(container);

            SetLocalVersion();
            SetRemoteVersionFile();
            SetReleaseFile();
        }

        public void SetLocalVersion()
        {
            var version = Assembly.GetEntryAssembly().GetName().Version ?? new Version(0, 0);
            LocalVersion = version;
        }

        public void SetRemoteVersionFile()
        {
            var blobClient = containerClient.GetBlobClient(versionFilename);
            string json = "";

            if (blobClient.Exists())
            {
                var response = blobClient.Download();
                using var streamReader = new StreamReader(response.Value.Content);
                
                while (!streamReader.EndOfStream)
                {
                    json += streamReader.ReadLine();
                }
            }

            RemoteVersion = JsonConvert.DeserializeObject<VersionFile>(json);
        }

        public void SetReleaseFile()
        {
            // get package
            var blobClient = containerClient.GetBlobClient(RemoteVersion.Latest);

            if (blobClient.Exists())
            {
                var response = blobClient.Download();
                Stream = response.Value.Content;
            }
        }


        // Publish


    }
}
