using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate.Provider
{
    class LocalAssemblyVersionProvider : IVersionProvider
    {
        public Task<Version> GetVersionAsync()
        {
            var version = Assembly.GetEntryAssembly().GetName().Version ?? new Version(0, 0);
            return Task.FromResult(version);
        }

        public Task SetVersionAsync(Version version)
        {
            Console.WriteLine("[WARNING] Set own project version is not possible");
            return Task.CompletedTask;
        }
    }
}
