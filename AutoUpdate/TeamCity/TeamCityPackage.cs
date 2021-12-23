using AutoUpdate.Models;
using AutoUpdate.Package;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate.TeamCity
{
    public class TeamCityPackage : IPackage
    {
        private readonly ITeamCityApi _client;

        public TeamCityPackage(ITeamCityApi client)
        {
            _client = client;
        }

        public async Task<byte[]> GetContentAsync(Version version, EventHandler<ProgressDownloadEvent> handler)
        {
            return _client.GetLatestPackage();
        }

        public Task SetContentAsync(byte[] data, Version version, EventHandler<ProgressUploadEvent> handler)
        {
            throw new NotImplementedException("Not possible to set Any new package to TeamCity");
        }

    }
}
