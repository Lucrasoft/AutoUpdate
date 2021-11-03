using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate
{
    public interface IUpdater
    {
        
        /// <summary>
        /// Checks if the remote version is newer than the local version. 
        /// </summary>
        /// <returns>True if remote version is newer.</returns>
        public Task<bool> UpdateAvailableAsync(Func<Version, Version, bool> updateMessageContinue);


        /// <summary>
        /// Performs an in-place update. 
        /// </summary>
        /// <returns></returns>
        public Task Update(Action<string, int> currentOperationTotalPercentDone);


        /// <summary>
        /// Restart current process. 
        /// </summary>
        public void Start(Func<List<string>> extraArguments);

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


        public void Publish();


    }
}
