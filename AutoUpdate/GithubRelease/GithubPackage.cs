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
            if(releases == null || releases.Count == 0 )
            {
                throw new ArgumentOutOfRangeException("No github release has been found.");
            }

            var release = releases[0];
            if(release.Assets != null && release.Assets.Count == 0)
            {
                throw new ArgumentOutOfRangeException("Has not found any github release assets.");
            }

            var asset = release.Assets[0];
            if (asset.BrowserDownloadUrl != null)
            {
                throw new MissingMemberException("Has not found the Uro of the github release assets.");
            }

            var downloadUrl = new Uri(asset.BrowserDownloadUrl);
            return (await PackageUtils.GetMemoryStreamForDownloadUrlAsync(downloadUrl, handler)).ToArray();
        }

        public Task SetContentAsync(byte[] data, Version version, EventHandler<ProgressUploadEvent> handler)
        {
            //filename = filename.Replace(".zip", "");
            throw new NotImplementedException("[WARNING] Pushing a release to Github is not implemented");
        }
    
    }
}
