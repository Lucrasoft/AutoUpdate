using AutoUpdate;
using AutoUpdate.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace OtherNamedConsole
{
    class Program
    {

        static async Task<int> Main(string[] args)
        {
            #region Chunk
            //var serviceCollection = new ServiceCollection();
            //ConfigureServices(serviceCollection);
            // var serviceProvider = serviceCollection.BuildServiceProvider();

            //Console.WriteLine("Hello World! From a branch");

            //#region generate AutoUpdate (output: au)
            //var auBuilder = new AutoUpdateBuilder();

            // auBuilder.SetHttpClient(new HttpClient());

            // auBuilder.AddUpdateType(PackageUpdateEnum.SideBySide);

            //auBuilder.AddGithub("https://github.com/niektuytel/HelloRelease");

            //auBuilder.AddBlobStorage(
            //    connectionString: "DefaultEndpointsProtocol=https;AccountName=teststorage777;AccountKey=Tq56DDVRLkmY6S/srcXoGsas6n1ao4fVeYYLdamWvR+Mxih4LZ6H2B3IBH40xv8AUGaAvOidcA+x6CcM9H5hrw==;EndpointSuffix=core.windows.net",
            //    container: "releases"
            //);

            //string website = "teamcity.lucrasoft.nl";
            //string token = "eyJ0eXAiOiAiVENWMiJ9.YVJLb0dsLV8xQ3pCdXo1dEZDa3JZUXZYaVdz.YWM3YmI4NTEtY2M3Ni00NDk0LTg0ZmUtZTk3ZmU3MDFhZWNi";
            //string buildTypeId = "AfasSystems_DevelopmentBuilds";

            //auBuilder.AddTeamCity(website, token, buildTypeId);

            ////auBuilder.AddGithub("https://github.com/niektuytel/HelloRelease");

            ////auBuilder.AddPackage(@"C:\Users\n.tuytel\Resources\AutoUpdate\Sample\bin\Debug\1.0.38.0.zip");
            //////auBuilder.LocalVersion(@"C:\Users\n.tuytel\Resources\AutoUpdate\Sample\bin\Debug\net5.0\samples\sample1\version.xml");
            //////auBuilder.LocalVersion(@"C:\Users\n.tuytel\Resources\AutoUpdate\Sample\bin\Debug\net5.0\samples\sample1\version.txt");
            ////auBuilder.LocalVersion(@"C:\Users\n.tuytel\Resources\AutoUpdate\Sample\bin\Debug\net5.0\samples\sample1\version.json");
            ////auBuilder.RemoteVersion(@"C:\Users\n.tuytel\Resources\AutoUpdate\Sample\bin\Debug\net5.0\samples\sample1\version.xml");
            //////auBuilder.RemoteVersion(@"C:\Users\n.tuytel\Resources\AutoUpdate\Sample\bin\Debug\net5.0\samples\sample1\version.txt");
            //////auBuilder.RemoteVersion(@"C:\Users\n.tuytel\Resources\AutoUpdate\Sample\bin\Debug\net5.0\samples\sample1\version.json");

            #endregion

            var tcWebsite = "teamcity.lucrasoft.nl";
            var tcToken = "eyJ0eXAiOiAiVENWMiJ9.YVJLb0dsLV8xQ3pCdXo1dEZDa3JZUXZYaVdz.YWM3YmI4NTEtY2M3Ni00NDk0LTg0ZmUtZTk3ZmU3MDFhZWNi";
            var tcBuildTypeID = "AfasSystems_DevelopmentBuilds";
            var au = new AutoUpdateBuilder()
                .AddUpdateType(PackageUpdateEnum.SideBySide)
                .AddTeamCity(tcWebsite, tcToken, tcBuildTypeID)
                .Build();

            //Update
            if (await au.UpdateAvailableAsync())
            {
                Console.WriteLine("AutoUpdate: found new version. Updating...");

                var result = await au.Update();
                if (result == 0)
                {
                    return au.Restart();
                }
            }
            else
            {
                Console.WriteLine("AutoUpdate: no update found.");
            }

            //// Publish
            //if (await au.PublishAvailableAsync())
            //{
            //    Console.WriteLine("AutoUpdate: has newer version. publishing...");
            //    await au.Publish();
            //}
            //else
            //{
            //    Console.WriteLine("AutoUpdate: has no newer version.");
            //}

            return 0;
        }

    }

}

