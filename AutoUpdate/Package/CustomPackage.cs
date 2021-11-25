using AutoUpdate.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate.Package
{
    class CustomPackage : IPackage
    {
        private byte[] bytes;

        public CustomPackage(byte[] bytes)
        {
            this.bytes = bytes;
        }

        public Task<byte[]> GetContentAsync(Version version, EventHandler<ProgressDownloadEvent> handler)
        {
            return Task.FromResult(this.bytes);
        }

        public Task SetContentAsync(byte[] data, Version version, EventHandler<ProgressUploadEvent> handler)
        {
            this.bytes = data;
            return Task.CompletedTask;
        }
    }
}
