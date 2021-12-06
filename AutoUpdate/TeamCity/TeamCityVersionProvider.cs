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
        private readonly string Host;
        private readonly string Token;
        private readonly string BuildTypeID;

        public TeamCityVersionProvider(string host, string token, string buildTypeId)
        {
            Host = host;
            Token = token;
            BuildTypeID = buildTypeId;
        }

        public async Task<Version> GetVersionAsync()
        {
            // Get last build
            var url = $"https://{Host}/app/rest/buildTypes/id:{BuildTypeID}/builds/running:false,status:success";
            var response = HttpRequest(url);
            if (!response.IsSuccessful)
            {
                throw new Exception($"There is no build available. id:{BuildTypeID} on url:{url}");
            }

            // Get version number
            var lastBuild = JsonConvert.DeserializeObject<LastBuild>(response.Content);
            return VersionConverter.Getversion(lastBuild.number);
        }

        private IRestResponse HttpRequest(string url)
        {
            var client = new RestClient(url)
            {
                Timeout = -1
            };

            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", $"Bearer {Token}");
            return client.Execute(request);
        }

        public async Task SetVersionAsync(Version version)
        {
            throw new NotImplementedException("Not possible to set Any new version to TeamCity");
        }

    }
}
