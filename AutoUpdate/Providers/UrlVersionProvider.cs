using Octokit;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate.Provider
{
    class UrlVersionProvider : IVersionProvider
    {
        private readonly Uri remoteurl;


        public UrlVersionProvider(Uri url)
        {
            remoteurl = url;
        }

        public async Task<Version> GetVersionAsync()
        {
            var version = new Version(0, 0, 0, 0);
            var response = await Updater.HTTPClient.GetAsync(remoteurl);
            if (!response.IsSuccessStatusCode) return version;

            // read version
            var content = await response.Content.ReadAsStringAsync();
            switch (response.Content.Headers.ContentType.MediaType.ToLower())
            {
                // { "version":"1.0.0.0" }
                case "application/json":
                    version = new JsonToVersionReader().GetVersion(content);
                    break;

                // <version>1.0.0.0</version>
                case "application/xml":
                    version = new XmlToVersionReader().GetVersion(content);
                    break;

                // 1.0.0.0
                case "text/plain":
                    version = new StringToVersionReader().GetVersion(content);
                    break;
            }

            return version;
        }

        public Task SetVersionAsync(Version version)
        {
            Console.WriteLine("[WARNING] Set version of URL is not possible");
            return Task.CompletedTask;
        }
    }
}
