using AutoUpdate.Package;
using AutoUpdate.Providers;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Octokit;
using Sample;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate
{

    /// <summary>
    /// A builder/helper to setup the correct IUpdater instance.
    /// </summary>
    public class AutoUpdateBuilder
    {
        private IVersionProvider local;
        private IVersionProvider remote;
        private IPackage package;

        public AutoUpdateBuilder()
        {
            //default version providers..
            this.local = new LocalAssemblyVersionProvider();
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
        /// <param name="format">Indicates if the content contains json, xml or text.</param>
        /// <returns></returns>
        public AutoUpdateBuilder LocalVersion(string filename, VersionFormat format = VersionFormat.AutoDetect)
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
        /// <param name="format">Indicates if the content contains json, xml or text.</param>
        /// <returns></returns>
        public AutoUpdateBuilder RemoteVersion(string filename, VersionFormat format = VersionFormat.AutoDetect)
        {
            this.remote = new FileVersionProvider(filename);
            return this;
        }

        /// <summary>
        /// A HTTP URI which returns the version information
        /// </summary>
        /// <param name="url"></param>
        /// <param name="format">Indicates if the content contains json, xml or text.</param>
        /// <returns></returns>
        public AutoUpdateBuilder RemoteVersion(Uri url, VersionFormat format = VersionFormat.AutoDetect)
        {
            this.remote = new UrlVersionProvider(url);
            return this;
        }


        /// <summary>
        /// Provide a HTTP URI which contents should contain a zip archive.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public AutoUpdateBuilder AddPackage( Uri url )
        {
            package = new DownloadPackage(url);
            return this;
        }

        /// <summary>
        /// Provide a version-controlled HTTP URI. The URI should give back a zip archive as content.
        /// </summary>
        /// <param name="UrlFunction"></param>
        /// <returns></returns>
        public AutoUpdateBuilder AddPackage( Func<Version, Uri> UrlFunction) 
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





        // Bv.Builder.AddBlobStorage(storageaccount, folder); waarin automatisch geregeld is hoe de remote version werkt, en hoe de bestandsnaam opgebouwd is.
        // Een Azure Blob Storage add-on
        // de remote version is nu een vaste bestands naam waar bv een version.json bestand staat + een application.x.y.z.zip bestand.
        // de UPDATE kant, weet hoe die die kan vinden.
        // de autoupdate PUBLISH kant, weet hoe die een nieuwe versie (zichzelf) daar kan publiceren, door zowel een zip van zichzelf te maken, deze te uploaden naar de blobstorage met de juiste naam, en de version.json in de blobstorage aan te passen.

        /// <summary>
        /// Provide the url of a github repository.
        /// </summary>
        /// <param name="url">Github repo url.</param>
        /// <returns></returns>
        public AutoUpdateBuilder AddBlobStorageAsync(string connectionString, string container)
        {
            var blob = new BlobStorage(connectionString, container);
            

            return this;
        }


        /// <summary>
        /// Provide the url of a github repository.
        /// </summary>
        /// <param name="url">Github repo url.</param>
        /// <returns></returns>
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

                //return this;
            }

            var owner = urlpaths[0];
            var repo = urlpaths[1];
            remote = new GithubVersionProvider(owner, repo);

            package = new DownloadPackage((v) =>
                {
                    var client = new GitHubClient(new ProductHeaderValue(repo));
                    var releases = client.Repository.Release.GetAll(owner, repo);
                    releases.Wait();

                    var release = releases.Result[0];
                    var packageurl = release.Assets[0].BrowserDownloadUrl;
                    return new Uri(packageurl);
                }
            );

            return this;
        }


        /// <summary>
        /// Build the IUpdater instance.
        /// </summary>
        /// <returns></returns>
        public IUpdater Build()
        {
            if (remote==null || package == null)
            {
                throw new ArgumentNullException("You must specifiy a remote version provider .");
            }

            return new Updater(local, remote, package);
        }


        //public void DownloadProgressEvent(DownloadProgressEventArgs process)
        //{
        //}


        ////This delegate can be used to point to methods
        ////which return void and take a string.
        //public delegate void MyEventHandler(string foo);

        ////This event can cause any method which conforms
        ////to MyEventHandler to be called.
        //public event MyEventHandler SomethingHappened;

        ////Here is some code I want to be executed
        ////when SomethingHappened fires.
        //void HandleSomethingHappened(string foo)
        //{
        //    //Do some stuff
        //}

        ////I am creating a delegate (pointer) to HandleSomethingHappened
        ////and adding it to SomethingHappened's list of "Event Handlers".
        //myObj.SomethingHappened += new MyEventHandler(HandleSomethingHappened);

        ////To raise the event within a method.
        //SomethingHappened("bar");



    }
}
