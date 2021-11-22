using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate.Providers
{
    class LocalAssemblyVersionProvider : IVersionProvider
    {



        public Task<Version> GetVersionAsync()
        {
            var version = Assembly.GetEntryAssembly().GetName().Version ?? new Version(0, 0);
            return Task.FromResult(version);
        }

        public async Task SetVersionAsync(Version version)
        {
            throw new NotImplementedException();
        }

    }
}
