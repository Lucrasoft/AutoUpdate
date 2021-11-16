using AutoUpdate.Models;
using AutoUpdate.Package;
using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate.BlobStorage
{
    class BlobStoragePackage : IPackage
    {
        public BlobContainerClient ContainerClient { get; set; }

        public BlobClient Client { get; set; }

        public BlobStoragePackage(BlobContainerClient containerClient)
        {
            ContainerClient = containerClient;
        }

        public async Task<byte[]> GetContentAsync(Version version, EventHandler<DownloadProgressEventArgs> handler)
        {
            Client = ContainerClient.GetBlobClient($"{version}.zip");

            if (Client.Exists())
            {
                var response = await Client.DownloadAsync();
                return PackageUtils.FillFromRemoteStream(response.Value.Content).ToArray();
            }

            return null;
        }

    }
}
