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
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate
{
    /// <summary>
    /// A builder/helper to setup the correct IUpdater instance.
    /// </summary>
    public class AutoUpdateBuilder
    {
        private HttpClient httpClient;
        private PackageUpdateEnum updateType = PackageUpdateEnum.InPlace;
        private IVersionProvider local;
        private IVersionProvider remote;
        private ILogger _logger = NullLogger.Instance;
        private IPackage package;


        public AutoUpdateBuilder()
        { }

        /// <summary>
        /// Add logger to system
        /// </summary>
        /// <param name="logger">Used to log whole application with</param>
        public AutoUpdateBuilder AddLogger(ILogger logger)
        {
            _logger = logger;
            return this;
        }

        /// <summary>
        /// Set HttpClient than the whole application contains the same configurations.
        /// </summary>
        /// <param name="client">The HttpClient that will been used globally</param>
        public AutoUpdateBuilder AddHttpClient(HttpClient client)
        {
            httpClient = client;
            return this;
        }

        /// <summary>
        /// The type of update would infect the way of how the new versions are downloaded.<br/>
        /// </summary>
        /// <param name="type">The way how to download new releases</param>
        public AutoUpdateBuilder AddUpdateType(PackageUpdateEnum type)
        {
            updateType = type;
            return this;
        }

        /// <summary>
        /// Manually supply a local version
        /// </summary>
        /// <param name="version">Local defined version</param>
        public AutoUpdateBuilder AddLocalVersion(Version version)
        {
            this.local = new CustomVersionProvider(version);
            return this;
        }

        /// <summary>
        /// A file containing the version information
        /// </summary>
        /// <param name="filename">Local defined version</param>
        public AutoUpdateBuilder AddLocalVersion(string filename)
        {
            this.local = new FileVersionProvider(filename);
            return this;
        }

        /// <summary>
        /// Manully supply a remote version.
        /// </summary>
        /// <param name="version">Remote defined version</param>
        public AutoUpdateBuilder AddRemoteVersion(Version version)
        {
            this.remote = new CustomVersionProvider(version);
            return this;
        }

        /// <summary>
        /// A file containing the version information.
        /// </summary>
        /// <param name="filename">Remote defined version</param>
        public AutoUpdateBuilder AddRemoteVersion(string filename)
        {
            this.remote = new FileVersionProvider(filename);
            return this;
        }

        /// <summary>
        /// A HTTP URI which returns the version information
        /// </summary>
        /// <param name="url">Remote defined version</param>
        public AutoUpdateBuilder AddRemoteVersion(Uri url)
        {
            this.remote = new UrlVersionProvider(url);
            return this;
        }

        /// <summary>
        /// Provide a HTTP URI which contents should contain a zip archive.
        /// </summary>
        /// <param name="url">Uri url</param>
        public AutoUpdateBuilder AddPackage(Uri url)
        {
            package = new DownloadPackage(url);
            return this;
        }

        /// <summary>
        /// Provide a version-controlled HTTP URI. The URI should give back a zip archive as content.
        /// </summary>
        /// <param name="UrlFunc">Url function to define url</param>
        public AutoUpdateBuilder AddPackage(Func<Version, Uri> UrlFunc) 
        {
            package = new DownloadPackage(UrlFunc);
            return this;
        }

        /// <summary>
        /// Provide the content of a zip archive.
        /// </summary>
        /// <param name="zipContents"></param>
        public AutoUpdateBuilder AddPackage(byte[] zipContents)
        {
            package = new CustomPackage(zipContents);
            return this;
        }

        /// <summary>
        /// Provide the name of a zip file.
        /// </summary>
        /// <param name="filename"></param>
        public AutoUpdateBuilder AddPackage(string filename)
        {
            package = new FilePackage(filename);
            return this;
        }

        /// <summary>
        /// Provide the url of the azure blob storage.
        /// </summary>
        /// <param name="connectionString">Connection string of storage account.</param>
        /// <param name="container">Blob container name.</param>
        public AutoUpdateBuilder AddBlobStorage(string connectionString, string container)
        {
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
            if(!containerClient.Exists())
            {
                throw new ArgumentException($"Container: {container} does not exist. in given blob storage account");
            }

            remote = new BlobStorageVersionProvider(containerClient);
            package = new BlobStoragePackage(containerClient);

            return this;
        }

        /// <summary>
        /// Can use github release to download current version from.
        /// </summary>
        /// <param name="url">Github repo url.</param>
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

        /// <summary>
        /// Can use team city to download current version from.
        /// </summary>
        /// <param name="website">The TeamCity domain (sample: "teamcity.website.nl")</param>
        /// <param name="token">The token to access project in TeamCity.</param>
        /// <param name="buildTypeId">Chosen type of build to use</param>
        public AutoUpdateBuilder AddTeamCity(string website, string token, string buildTypeId) 
        {
            // Exceptions: when http call failed.
            var client = new TeamCityApi(website, token, buildTypeId);

            remote = new TeamCityVersionProvider(client);
            package = new TeamCityPackage(client);

            return this;
        }

        /// <summary>
        /// Build the IUpdater instance.
        /// </summary>
        /// <returns>Updater class</returns>
        public IUpdater Build()
        {
            if (remote == null || package == null)
            {
                throw new ArgumentNullException("You must specifiy a remote version provider .");
            }

            // default local version provider..
            if(local == null)
            {
                local = new LocalAssemblyVersionProvider(_logger);
            }

            // warn on logger asigned
            if(_logger == NullLogger.Instance)
            {
                Console.WriteLine(
                    "[INFO] AutoUpdate Logs are prevented. (add: .AddLogger(ILogger) to enable)"
                );
            }

            return new Updater(local, remote, package, updateType, httpClient, _logger);
        }

    }
}
