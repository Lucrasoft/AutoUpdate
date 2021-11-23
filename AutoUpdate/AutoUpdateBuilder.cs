using AutoUpdate.BlobStorage;
using AutoUpdate.GithubRelease;
using AutoUpdate.Models;
using AutoUpdate.Package;
using AutoUpdate.Provider;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Octokit;
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
        private HttpClient httpClient = new();
        private PackageUpdateEnum packageUpdateType = PackageUpdateEnum.InPlace;

        private IVersionProvider local;
        private IVersionProvider remote;
        private IPackage package;


        public AutoUpdateBuilder()
        {
            //default version providers..
            local = new LocalAssemblyVersionProvider();
        }

        /// <summary>
        /// Set HttpClient than the whole application contains the same configurations.
        /// </summary>
        /// <param name="client">The HttpClient that will been used globally</param>
        /// <returns></returns>
        public AutoUpdateBuilder SetHttpClient(HttpClient client)
        {
            httpClient = client;
            return this;
        }

        /// <summary>
        /// The type of update would infect the way of how the new versions are downloaded.<br/>
        /// </summary>
        /// <param name="type">The way how to download new releases</param>
        /// <returns></returns>
        public AutoUpdateBuilder SetPackageUpdateType(PackageUpdateEnum type)
        {
            packageUpdateType = type;
            return this;
        }

        /// <summary>
        /// Manually supply a local version
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public AutoUpdateBuilder LocalVersion(Version version)
        {
            this.local = new CustomVersionProvider(version);
            return this;
        }

        /// <summary>
        /// A file containing the version information
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public AutoUpdateBuilder LocalVersion(string filename)
        {
            this.local = new FileVersionProvider(filename);
            return this;
        }

        /// <summary>
        /// Manully supply a remote version.
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public AutoUpdateBuilder RemoteVersion(Version version)
        {
            this.remote = new CustomVersionProvider(version);
            return this;
        }

        /// <summary>
        /// A file containing the version information.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public AutoUpdateBuilder RemoteVersion(string filename)
        {
            this.remote = new FileVersionProvider(filename);
            return this;
        }

        /// <summary>
        /// A HTTP URI which returns the version information
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public AutoUpdateBuilder RemoteVersion(Uri url)
        {
            this.remote = new UrlVersionProvider(url);
            return this;
        }

        /// <summary>
        /// Provide a HTTP URI which contents should contain a zip archive.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public AutoUpdateBuilder AddPackage(Uri url)
        {
            package = new DownloadPackage(url);
            return this;
        }

        /// <summary>
        /// Provide a version-controlled HTTP URI. The URI should give back a zip archive as content.
        /// </summary>
        /// <param name="UrlFunction"></param>
        /// <returns></returns>
        public AutoUpdateBuilder AddPackage(Func<Version, Uri> UrlFunction) 
        {
            package = new DownloadPackage(UrlFunction);
            return this;
        }

        /// <summary>
        /// Provide the content of a zip archive.
        /// </summary>
        /// <param name="zipContents"></param>
        /// <returns></returns>
        public AutoUpdateBuilder AddPackage(byte[] zipContents)
        {
            package = new CustomPackage(zipContents);
            return this;
        }

        /// <summary>
        /// Provide the name of a zip file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
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
        /// <returns>AutoUpdateBuilder class</returns>
        public AutoUpdateBuilder AddBlobStorage(string connectionString, string container)
        {
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(container);

            remote = new BlobStorageVersionProvider(containerClient);
            package = new BlobStoragePackage(containerClient);

            return this;
        }

        /// <summary>
        /// Can use a global HttpClient than can contain settings.
        /// </summary>
        /// <param name="url">Github repo url.</param>
        /// <returns>AutoUpdateBuilder class</returns>
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
            remote = new GithubVersionProvider(owner, repo);
            package = new GithubPackage(owner, repo);

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

            return new Updater(local, remote, package, packageUpdateType, httpClient);
        }

    }
}
