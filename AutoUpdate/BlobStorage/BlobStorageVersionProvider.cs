using AutoUpdate.Models;
using AutoUpdate.Provider;
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
using static AutoUpdate.Provider.JsonToVersionReader;

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
            if (await Client.ExistsAsync())
            {
                var response = await Client.DownloadAsync();
                using var streamReader = new StreamReader(response.Value.Content);

                while (!streamReader.EndOfStream)
                {
                    json += await streamReader.ReadLineAsync();
                }
            }

            return new JsonToVersionReader().GetVersion(json);
        }

        public async Task SetVersionAsync(Version version)
        {
            var file = new VersionObject()
            {
                version = PackageUtils.GetVersionString(version)
            };
            var data = PackageUtils.GenerateStream(file);

            // Upload data from the local file
            await Client.UploadAsync(data, true);
        }

    }
}
