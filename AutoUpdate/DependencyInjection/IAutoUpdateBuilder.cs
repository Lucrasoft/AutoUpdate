using AutoUpdate.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;

namespace AutoUpdate.DependencyInjection
{
    public interface IAutoUpdateBuilder
    {
        /// <summary>
        /// Files that will been replaced and keep the old values with the same key value.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        AutoUpdateBuilder AddIgnoringJsonFile(string filename);

        /// <summary>
        /// Add logger to system
        /// </summary>
        /// <param name="logger">Used to log whole application with</param>
        AutoUpdateBuilder AddLogger(ILogger logger);
        
        /// <summary>
        /// Provide the url of the azure blob storage.
        /// </summary>
        AutoUpdateBuilder AddBlobStorage();

        /// <summary>
        /// Can use github release to download current version from.
        /// </summary>
        /// <param name="url">Github repo url.</param>
        AutoUpdateBuilder AddGithub(string url);

        /// <summary>
        /// Set HttpClient than the whole application contains the same configurations.
        /// </summary>
        /// <param name="client">The HttpClient that will been used globally</param>
        AutoUpdateBuilder AddHttpClient(HttpClient client);

        /// <summary>
        /// A file containing the version information
        /// </summary>
        /// <param name="filename">Local defined version</param>
        AutoUpdateBuilder AddLocalVersion(string filename);

        /// <summary>
        /// Manually supply a local version
        /// </summary>
        /// <param name="version">Local defined version</param>
        AutoUpdateBuilder AddLocalVersion(Version version);

        /// <summary>
        /// Provide the content of a zip archive.
        /// </summary>
        /// <param name="zipContents"></param>
        AutoUpdateBuilder AddPackage(byte[] zipContents);

        /// <summary>
        /// Provide a version-controlled HTTP URI. The URI should give back a zip archive as content.
        /// </summary>
        /// <param name="UrlFunc">Url function to define url</param>
        AutoUpdateBuilder AddPackage(Func<Version, Uri> UrlFunc);

        /// <summary>
        /// Provide the name of a zip file.
        /// </summary>
        /// <param name="filename"></param>
        AutoUpdateBuilder AddPackage(string filename);

        /// <summary>
        /// Provide a HTTP URI which contents should contain a zip archive.
        /// </summary>
        /// <param name="url">Uri url</param>
        AutoUpdateBuilder AddPackage(Uri url);

        /// <summary>
        /// A file containing the version information.
        /// </summary>
        /// <param name="filename">Remote defined version</param>
        AutoUpdateBuilder AddRemoteVersion(string filename);

        /// <summary>
        /// A HTTP URI which returns the version information
        /// </summary>
        /// <param name="url">Remote defined version</param>
        AutoUpdateBuilder AddRemoteVersion(Uri url);

        /// <summary>
        /// Manully supply a remote version.
        /// </summary>
        /// <param name="version">Remote defined version</param>
        AutoUpdateBuilder AddRemoteVersion(Version version);

        /// <summary>
        /// Can use team city to download current version from.
        /// </summary>
        AutoUpdateBuilder AddTeamCity();

        /// <summary>
        /// The type of update would infect the way of how the new versions are downloaded.<br/>
        /// </summary>
        /// <param name="type">The way how to download new releases</param>
        AutoUpdateBuilder AddUpdateType(PackageUpdateEnum type);

        /// <summary>
        /// Build the IUpdater instance.
        /// </summary>
        /// <returns>Updater class</returns>
        IUpdater Build();
    }
}