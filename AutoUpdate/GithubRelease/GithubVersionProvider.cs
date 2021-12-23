using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate.Provider
{
    public class GithubVersionProvider : IVersionProvider
    {
        private readonly string owner;
        private readonly string repo;
        private readonly GitHubClient client;
        private readonly ILogger logger;

        public GithubVersionProvider(string owner, string repo, ILogger logger)
        {
            this.owner = owner;
            this.repo = repo;
            this.logger = logger;

            client = new GitHubClient(new ProductHeaderValue(repo));
            // TODO: extend limit call
            //{
            //    // Bypass the limit calls to github.
            //    Credentials = new Credentials(username, password)
            //};
        }

        public async Task<Version> GetVersionAsync()
        {
            var version = new Version();

            try
            {
                var releases = await client.Repository.Release.GetAll(owner, repo);
                var release = releases[0];

                version = new Version(release.TagName);
            }
            catch (Exception e)
            {
                logger.LogError(
                    $"(GithubVersionProvider::GetVersionAsync) GitHubClient Exception \n" + 
                    $"[ERROR] Message:{e.Message}\n"
                );
            }

            return version;
        }

        public Task SetVersionAsync(Version version)
        {
            throw new NotImplementedException("[WARNING] Pushing a version to Github is not implemented");
        }
    }
}
