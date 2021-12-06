﻿using AutoUpdate.Models;
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
        private BlobContainerClient containerClient { get; set; }

        private BlobClient blobClient { get; set; }

        public BlobStoragePackage(BlobContainerClient containerClient)
        {
            this.containerClient = containerClient;
        }

        public async Task<byte[]> GetContentAsync(Version version, EventHandler<ProgressDownloadEvent> handler)
        {
            blobClient = containerClient.GetBlobClient($"{version}.zip");

            if (await blobClient.ExistsAsync())
            {
                var response = await blobClient.DownloadAsync();
                return PackageUtils.FillFromRemoteStream(response.Value.Content).ToArray();
            }

            return null;
        }

        public async Task SetContentAsync(byte[] data, Version version, EventHandler<ProgressUploadEvent> handler)
        {
            using var stream = new MemoryStream();
            await stream.WriteAsync(data.AsMemory(0, data.Length));
            stream.Position = 0;

            // Get a reference to a blob
            blobClient = containerClient.GetBlobClient($"{PackageUtils.GetVersionString(version)}.zip");

            // Upload data from the local file
            await blobClient.UploadAsync(stream, true);
        }
    
    }
}
