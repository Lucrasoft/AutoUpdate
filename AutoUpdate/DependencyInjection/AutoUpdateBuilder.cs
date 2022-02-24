using AutoUpdate.BlobStorage;
using AutoUpdate.GithubRelease;
using AutoUpdate.Models;
using AutoUpdate.Package;
using AutoUpdate.Provider;
using AutoUpdate.TeamCity;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Octokit;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate.DependencyInjection
{
    /// <summary>
    /// A builder/helper to setup the correct IUpdater instance.
    /// </summary>
    public class AutoUpdateBuilder : IAutoUpdateBuilder
    {
        private List<string> _ignoredFilenames = new();
        private HttpClient httpClient;
        private ILogger _logger = NullLogger.Instance;
        private PackageUpdateEnum updateType = PackageUpdateEnum.InPlace;
        private IVersionProvider local;
        private IVersionProvider remote;
        private IPackage package;

        private readonly IConfiguration _configuration;

        public AutoUpdateBuilder(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public AutoUpdateBuilder AddIgnoringJsonFile(string filename)
        {
            if (string.IsNullOrEmpty(filename) || !filename.EndsWith(".json"))
            {
                throw new MissingFieldException($"Failed adding ignored Json file: {filename}");
            }
            else if (!File.Exists(filename))
            {
                throw new Exception($"File can't been found. {filename}");
            }

            _ignoredFilenames.Add(filename);
            return this;
        }

        public AutoUpdateBuilder AddLogger(ILogger logger)
        {
            _logger = logger;
            return this;
        }

        public AutoUpdateBuilder AddHttpClient(HttpClient client)
        {
            httpClient = client;
            return this;
        }

        public AutoUpdateBuilder AddUpdateType(PackageUpdateEnum type)
        {
            updateType = type;
            return this;
        }

        public AutoUpdateBuilder AddLocalVersion(Version version)
        {
            this.local = new CustomVersionProvider(version);
            return this;
        }

        public AutoUpdateBuilder AddLocalVersion(string filename)
        {
            this.local = new FileVersionProvider(filename);
            return this;
        }

        public AutoUpdateBuilder AddRemoteVersion(Version version)
        {
            this.remote = new CustomVersionProvider(version);
            return this;
        }

        public AutoUpdateBuilder AddRemoteVersion(string filename)
        {
            this.remote = new FileVersionProvider(filename);
            return this;
        }

        public AutoUpdateBuilder AddRemoteVersion(Uri url)
        {
            this.remote = new UrlVersionProvider(url);
            return this;
        }

        public AutoUpdateBuilder AddPackage(Uri url)
        {
            package = new DownloadPackage(url);
            return this;
        }

        public AutoUpdateBuilder AddPackage(Func<Version, Uri> UrlFunc)
        {
            package = new DownloadPackage(UrlFunc);
            return this;
        }

        public AutoUpdateBuilder AddPackage(byte[] zipContents)
        {
            package = new CustomPackage(zipContents);
            return this;
        }

        public AutoUpdateBuilder AddPackage(string filename)
        {
            package = new FilePackage(filename);
            return this;
        }

        public AutoUpdateBuilder AddBlobStorage()
        {
            var connectionString = _configuration["BlobConnectionString"];
            var container = _configuration["BlobContainer"];
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new MissingFieldException("Missing `BlobConnectionString` key value pair in configurations.");
            }
            else if (string.IsNullOrEmpty(container))
            {
                throw new MissingFieldException("Missing `BlobContainer` key value pair in configurations.");
            }

            BlobServiceClient blobServiceClient;
            try
            {
                blobServiceClient = new BlobServiceClient(connectionString);
            }
            catch (FormatException)
            {
                throw new FormatException(
                    $"Correct connectionString format will be: 'DefaultEndpointsProtocol=...;AccountName=...;AccountKey=...;EndpointSuffix=..."
                );
            }

            var containerClient = blobServiceClient.GetBlobContainerClient(container);
            if (!containerClient.Exists())
            {
                throw new ArgumentException($"Container: {container} does not exist. in given blob storage account");
            }

            remote = new BlobStorageVersionProvider(containerClient);
            package = new BlobStoragePackage(containerClient);

            return this;
        }

        public AutoUpdateBuilder AddGithub(string url)
        {
            var uri = new Uri(url);
            var invalid = !uri.Host.Contains("github");
            var urlpaths = uri.AbsolutePath.Split("/")[1..];

            if (invalid || urlpaths.Length != 2)
            {
                throw new ArgumentException(
                    $"invalid: {url}. (hint: https://github.com/user/repo)"
                );
            }

            var owner = urlpaths[0];
            var repo = urlpaths[1];
            remote = new GithubVersionProvider(owner, repo, _logger);
            package = new GithubPackage(owner, repo);

            return this;
        }

        public AutoUpdateBuilder AddTeamCity()
        {
            var website = _configuration["TeamCityUrl"];
            var token = _configuration["TeamCityToken"];
            var buildTypeId = _configuration["TeamCityBuildTypeID"];
            if (string.IsNullOrEmpty(website))
            {
                throw new MissingFieldException("Missing `TeamCityUrl` key value pair in configurations.");
            }
            else if (string.IsNullOrEmpty(token))
            {
                throw new MissingFieldException("Missing `TeamCityToken` key value pair in configurations.");
            }
            else if (string.IsNullOrEmpty(buildTypeId))
            {
                throw new MissingFieldException("Missing `TeamCityBuildTypeID` key value pair in configurations.");
            }

            // Exceptions: when http call failed.
            var client = new TeamCityApi(website, token, buildTypeId);

            remote = new TeamCityVersionProvider(client);
            package = new TeamCityPackage(client);

            return this;
        }

        public IUpdater Build()
        {
            if (remote == null || package == null)
            {
                throw new ArgumentNullException("You must specifiy a remote version provider .");
            }

            // default local version provider..
            if (local == null)
            {
                local = new LocalAssemblyVersionProvider(_logger);
            }

            // warn on logger asigned
            if (_logger == NullLogger.Instance)
            {
                Console.WriteLine(
                    "[INFO] AutoUpdate Logs are prevented. (add: .AddLogger(ILogger) to enable)"
                );
            }

            return new Updater(local, remote, package, updateType, httpClient, _logger, _ignoredFilenames);
        }

    }
}
