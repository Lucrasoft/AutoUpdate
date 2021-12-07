using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate.Provider
{
    class LocalAssemblyVersionProvider : IVersionProvider
    {
        private readonly ILogger logger;


        public LocalAssemblyVersionProvider(ILogger logger)
        {
            this.logger = logger;
        }


        public Task<Version> GetVersionAsync()
        {
            var version = Assembly.GetEntryAssembly().GetName().Version ?? new Version(0, 0);
            return Task.FromResult(version);
        }

        public Task SetVersionAsync(Version version)
        {
            logger.LogWarning("Set own project version is not possible");
            return Task.CompletedTask;
        }
    }
}
