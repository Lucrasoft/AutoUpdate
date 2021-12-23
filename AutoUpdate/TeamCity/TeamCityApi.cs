using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate.TeamCity
{
    public class TeamCityApi : ITeamCityApi
    {
        private readonly string _host;
        private readonly string _token;
        private readonly string _buildTypeName;

        public TeamCityApi(string host, string token, string buildTypeName)
        {
            _host = host;
            _token = token;
            _buildTypeName = buildTypeName;
        }

        public byte[] GetLatestPackage()
        {
            var latestBuildID = GetLatestBuildID();

            var url = $"https://{_host}/app/rest/builds/id:{latestBuildID}/artifacts/archived";
            var response = HttpRequest(url);
            if (!response.IsSuccessful)
            {
                throw new Exception($"There is no build available on url:{url}\nCheck your credentials.");
            }

            return response.RawBytes;
        }

        public Version GetLatestVersion()
        {
            // Get last build
            var url = $"https://{_host}/app/rest/buildTypes/id:{_buildTypeName}/builds/running:false,status:success";
            var response = HttpRequest(url);
            if (!response.IsSuccessful)
            {
                throw new Exception($"There is no build available on url:{url}");
            }

            // Get version number
            var lastBuild = JsonConvert.DeserializeObject<LastBuild>(response.Content);
            if (lastBuild == null)
            {
                throw new Exception($"Build can't been converted to know model, response body is unknown");
            }

            return VersionConverter.Getversion(lastBuild.number);
        }

        private string GetLatestBuildID()
        {
            // Get last build
            var url = $"https://{_host}/app/rest/buildTypes/id:{_buildTypeName}/builds/running:false,status:success";
            var response = HttpRequest(url);
            if (!response.IsSuccessful)
            {
                throw new Exception($"HTTP call failed, on url:{url}");
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
            request.AddHeader("Authorization", $"Bearer {_token}");
            return client.Execute(request);
        }

    }




}
