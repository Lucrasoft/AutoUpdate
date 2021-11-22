using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate.Providers
{
    class CustomVersionProvider : IVersionProvider
    {

        private readonly Version version;

        public CustomVersionProvider(Version version)
        {
            this.version = version;
        }

        public Task<Version> GetVersionAsync()
        {
            return Task.FromResult(this.version);
        }

        public async Task SetVersionAsync(Version version)
        {
            throw new NotImplementedException();
        }
    }
}
