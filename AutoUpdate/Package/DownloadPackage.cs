using AutoUpdate.Models;
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

        //TODO centralize this HttpClient. 
        private static HttpClient httpClient = new HttpClient();

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

        public async Task<byte[]> GetContentAsync(Version version, EventHandler<DownloadProgressEventArgs> handler)
        {
            var url = downloadurl;
            if (downloadUrlFunc!=null)
            {
                url = downloadUrlFunc(version);
            };

            return (await PackageUtils.GetMemoryStreamForDownloadUrl(httpClient, url, handler)).ToArray();
        }
    }
}
