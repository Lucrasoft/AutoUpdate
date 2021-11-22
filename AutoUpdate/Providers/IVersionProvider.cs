using Octokit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate.Provider
{
    public interface IVersionProvider
    {

        Task<Version> GetVersionAsync();

        Task SetVersionAsync(Version version);

    }
}
