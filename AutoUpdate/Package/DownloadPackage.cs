﻿using AutoUpdate.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate.Package
{
    class DownloadPackage : IPackage
    {
        private readonly Uri downloadurl;
        private Func<Version, Uri> downloadUrlFunc;

        public DownloadPackage(Uri downloadurl)
        {
            this.downloadurl = downloadurl;
        }

        public DownloadPackage(Func<Version, Uri> downloadUrlFunc)
        {
            this.downloadUrlFunc = downloadUrlFunc;
        }

        public async Task<byte[]> GetContentAsync(Version version, EventHandler<ProgressDownloadEvent> handler)
        {
            var url = downloadurl;
            if (downloadUrlFunc!=null)
            {
                url = downloadUrlFunc(version);
            };

            return (await PackageUtils.GetMemoryStreamForDownloadUrl(url, handler)).ToArray();
        }

        public async Task SetContentAsync(byte[] data, Version version, EventHandler<ProgressUploadEvent> handler)
        {
            var url = downloadurl;
            if (downloadUrlFunc != null)
            {
                url = downloadUrlFunc(version);
            };

            await PackageUtils.PostMemoryStreamToDownloadUrl(new MemoryStream(data), PackageUtils.GetVersionString(version), url, handler);
        }
    }
}
