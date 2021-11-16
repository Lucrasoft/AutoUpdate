using Azure.Storage.Blobs;
using Newtonsoft.Json;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate.BlobStorage
{
    public class BlobStorageVersionProvider : IVersionProvider
    {
        private readonly string versionFilename = "version.json";

        public BlobClient Client;

        public BlobStorageVersionProvider(BlobContainerClient containerClient)
        {
            Client = containerClient.GetBlobClient(versionFilename);
            //if(Client.Exists()) { }
        }

        public async Task<Version> GetVersionAsync()
        {
            string json = "";

            if (Client.Exists())
            {
                var response = Client.Download();
                using var streamReader = new StreamReader(response.Value.Content);

                while (!streamReader.EndOfStream)
                {
                    json += streamReader.ReadLine();
                }
            }

            var version = JsonConvert.DeserializeObject<VersionFile>(json);
            return new Version(version.Latest);
        }

    }
}
