using AutoUpdate.Provider;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate.TeamCity
{
    public class TeamCityVersionProvider : IVersionProvider
    {
        private readonly ITeamCityApi _client;

        public TeamCityVersionProvider(ITeamCityApi client)
        {
            _client = client;
        }

        public async Task<Version> GetVersionAsync()
        {
            return _client.GetLatestVersion();
        }

        public async Task SetVersionAsync(Version version)
        {
            throw new NotImplementedException("Not possible to set Any new version to TeamCity");
        }

    }
}
