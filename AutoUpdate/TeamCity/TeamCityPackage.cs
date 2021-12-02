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
        private readonly string Host;
        private readonly string Token;
        private readonly string BuildTypeID;

        public TeamCityPackage(string host, string token, string buildTypeId)
        {
            Host = host;
            Token = token;
            BuildTypeID = buildTypeId;
        }

        public async Task<byte[]> GetContentAsync(Version version, EventHandler<ProgressDownloadEvent> handler)
        {
            var buildID = GetLatestBuildID();

            // Get last build
            var url = $"https://{Host}/app/rest/builds/id:{buildID}/artifacts/archived";
            var response = HttpRequest(url);
            if (!response.IsSuccessful)
            {
                throw new Exception($"There is no release available on id:{buildID} on url:{url}");
            }

            return response.RawBytes;
        }

        private string GetLatestBuildID()
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
            return lastBuild.id.ToString();
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

        public Task SetContentAsync(byte[] data, Version version, EventHandler<ProgressUploadEvent> handler)
        {
            throw new NotImplementedException("Not possible to set Any new package to TeamCity");
        }

    }
}
