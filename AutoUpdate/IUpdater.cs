using AutoUpdate.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate
{
    public interface IUpdater
    {
        /// <summary>
        /// Contains state of downloading
        /// </summary>
        public event EventHandler<ProgressDownloadEvent> OnDownloadProgress;

        /// <summary>
        /// Checks if the remote version is newer than the local version. 
        /// </summary>
        /// <returns>True if remote version is newer.</returns>
        public Task<bool> UpdateAvailableAsync(Func<Version, Version, bool> updateMessageContinue = null);

        /// <summary>
        /// Performs an in-place update. 
        /// </summary>
        /// <returns></returns>
        public Task<bool> Update(EventHandler<ProgressDownloadEvent> onDownloadProgress = null);

        /// <summary>
        /// Restart current process. 
        /// </summary>
        public bool Restart(Func<List<string>> extraArguments = null);

        /// <summary>
        /// Check if publishment is available.
        /// </summary>
        /// <param name="publishMessageContinue">Func than can influence publicity</param>
        /// <returns>
        /// Available to publish
        /// </returns>
        public Task<bool> PublishAvailableAsync(Func<Version, Version, bool> publishMessageContinue = null);

        /// <summary>
        /// Performs an in-place & side-by-side update. 
        /// </summary>
        /// <returns></returns>
        public Task Publish(EventHandler<ProgressUploadEvent> onUploadProgress = null);

        /// <summary>
        /// Returns the local version
        /// </summary>
        /// <returns></returns>
        public Task<Version> GetLocalVersion();

        /// <summary>
        /// Returns the remote version 
        /// </summary>
        /// <returns></returns>
        public Task<Version> GetRemoteVersion();

    }
}
