using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate.Provider
{
    class CustomVersionProvider : IVersionProvider
    {
        private Version version;

        public CustomVersionProvider(Version version)
        {
            this.version = version;
        }

        public Task<Version> GetVersionAsync()
        {
            return Task.FromResult(this.version);
        }

        public Task SetVersionAsync(Version version)
        {
            this.version = version;
            return Task.CompletedTask;
        }
    }
}
