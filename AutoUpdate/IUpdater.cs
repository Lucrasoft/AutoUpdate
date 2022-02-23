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
        #pragma warning disable VSTHRD200 // Use "Async" suffix for async methods
        public Task<int> Update(EventHandler<ProgressDownloadEvent> onDownloadProgress = null);

        /// <summary>
        /// Start new process using in-place or side-by-side 
        /// </summary>
        public int Restart(Func<List<string>> extraArguments = null, bool hasPrepareTimeThreshold = true);

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

        /// <summary>
        /// Update version provider made it less code on the client.
        /// </summary>
        /// <param name="action">response after updating</param>
        /// <param name="inDevMode">disable updating by settings it to development mode</param>
        /// <returns></returns>
        public Task UpdateProvider(Func<bool, int, Task> action, bool inDevMode = false);

    }
}
