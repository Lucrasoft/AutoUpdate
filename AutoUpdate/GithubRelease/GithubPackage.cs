using AutoUpdate.Models;
using AutoUpdate.Package;
using Octokit;
using System;
using System.Threading.Tasks;

namespace AutoUpdate.GithubRelease
{
    class GithubPackage : IPackage
    {
        private readonly string owner;
        private readonly string repo;
        private readonly GitHubClient client;

        public GithubPackage(string owner, string repo)
        {
            this.owner = owner;
            this.repo = repo;

            client = new GitHubClient(new ProductHeaderValue(repo));
        }

        public async Task<byte[]> GetContentAsync(Version version, EventHandler<ProgressDownloadEvent> handler)
        {
            var releases = await client.Repository.Release.GetAll(owner, repo);
            var release = releases[0];

            var downloadUrl = new Uri(release.Assets[0].BrowserDownloadUrl);
            return (await PackageUtils.GetMemoryStreamForDownloadUrl(downloadUrl, handler)).ToArray();
        }

        public async Task SetContentAsync(byte[] data, Version version, EventHandler<ProgressUploadEvent> handler)
        {
            //filename = filename.Replace(".zip", "");
            throw new NotImplementedException("[WARNING] Pushing a release to Github is not implemented");
        }
    
    }
}
