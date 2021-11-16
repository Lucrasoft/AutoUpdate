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
        private readonly string owner;
        private readonly string repo;

        public GithubVersionProvider(string owner, string repo)
        {
            this.owner = owner;
            this.repo = repo;
        }

        public async Task<Version> GetVersionAsync()
        {
            var client = new GitHubClient(new ProductHeaderValue(repo));
            var releases = await client.Repository.Release.GetAll(owner, repo);
            var release = releases[0];

            return new Version(release.TagName);
        }
    
    }
}
