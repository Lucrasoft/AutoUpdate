﻿using Octokit;
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
        private readonly GitHubClient client;

        public GithubVersionProvider(string owner, string repo)
        {
            this.owner = owner;
            this.repo = repo;

            client = new GitHubClient(new ProductHeaderValue(repo));
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
                Console.WriteLine(
                    $"(GithubVersionProvider::GetVersionAsync) GitHubClient Exception \n" + 
                    $"[ERROR] Message:{e.Message}\n"// +
                    // $"StackTrace: {e.StackTrace}"
                );
            }

            return version;
        }
    
    }
}
