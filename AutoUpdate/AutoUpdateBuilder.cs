using AutoUpdate.Package;
using AutoUpdate.Providers;
using Octokit;
using System;
using System.Collections.Generic;
using System.Text;

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


        


        ///// <summary>
        ///// Provide the url of a github repository.
        ///// </summary>
        ///// <param name="url">Github repo url.</param>
        ///// <returns></returns>
        //public AutoUpdateBuilder AddBlobStorage(Uri url)
        //{
        //    this.remote = new GithubVersionProvider(url);
        //
        //
        //    package = new DownloadPackage(UrlFunction);
        //    //package = new GithubPackage(url, remote);
        //
        //    return this;
        //}


        /// <summary>
        /// Provide the url of a github repository.
        /// </summary>
        /// <param name="url">Github repo url.</param>
        /// <returns></returns>
        public AutoUpdateBuilder AddGithubRelease(Uri url)
        {
            remote = new GithubVersionProvider(url);

            package = new DownloadPackage(
                (v) =>
                {
                    var version = $"{v.Major}.{v.Minor}";
                    
                    if (v.Build != -1)
                    {
                        version += $".{v.Build}";
                    }

                    if (v.Revision != -1)
                    {
                        version += $".{v.Revision}";
                    }

                    return new Uri($"{url.AbsoluteUri}/releases/download/{version}/release_{version}.zip");
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

    }
}
