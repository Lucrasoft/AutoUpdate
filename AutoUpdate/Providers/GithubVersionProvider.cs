using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate.Providers
{
    public class GithubVersionProvider : IVersionProvider
    {
        //private readonly Uri remoteurl;
        private readonly string[] urlpaths;

        public GithubVersionProvider(Uri url)
        {
            var invalid = !url.Host.Contains("github");
            urlpaths = url.AbsolutePath.Split("/")[1..];

            if (invalid || urlpaths.Length != 2)
            {
                throw new ArgumentException(
                    $"invalid: {url}. (hint: https://github.com/user/repo)"
                );
            }

            //this.remoteurl = url;
        }

        public async Task<Version> GetVersionAsync()
        {
            var client = new GitHubClient(new ProductHeaderValue(urlpaths[1]));
            var releases = await client.Repository.Release.GetAll(urlpaths[0], urlpaths[1]);
            var latest = releases[0];

            return new Version(latest.TagName);
        }
    
    }
}
