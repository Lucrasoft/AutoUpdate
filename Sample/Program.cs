using AutoUpdate;
using AutoUpdate.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace OtherNamedConsole
{
    class Program
    {

        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            #region generate AutoUpdate (output: au)
            var auBuilder = new AutoUpdateBuilder();

            //auBuilder.SetHttpClient(new HttpClient());

            //auBuilder.SetPackageUpdateType(PackageUpdateEnum.SideBySide);

            //auBuilder.AddBlobStorage(
            //    connectionString: "DefaultEndpointsProtocol=https;AccountName=teststorage777;AccountKey=Tq56DDVRLkmY6S/srcXoGsas6n1ao4fVeYYLdamWvR+Mxih4LZ6H2B3IBH40xv8AUGaAvOidcA+x6CcM9H5hrw==;EndpointSuffix=core.windows.net",
            //    container: "releases"
            //);

            //auBuilder.AddGithub("https://github.com/niektuytel/HelloRelease");

            auBuilder.AddPackage(@"C:\Users\n.tuytel\Resources\AutoUpdate\Sample\bin\Debug\1.0.38.0.zip");
            //auBuilder.LocalVersion(@"C:\Users\n.tuytel\Resources\AutoUpdate\Sample\bin\Debug\net5.0\samples\sample1\version.xml");
            //auBuilder.LocalVersion(@"C:\Users\n.tuytel\Resources\AutoUpdate\Sample\bin\Debug\net5.0\samples\sample1\version.txt");
            auBuilder.LocalVersion(@"C:\Users\n.tuytel\Resources\AutoUpdate\Sample\bin\Debug\net5.0\samples\sample1\version.json");
            auBuilder.RemoteVersion(@"C:\Users\n.tuytel\Resources\AutoUpdate\Sample\bin\Debug\net5.0\samples\sample1\version.xml");
            //auBuilder.RemoteVersion(@"C:\Users\n.tuytel\Resources\AutoUpdate\Sample\bin\Debug\net5.0\samples\sample1\version.txt");
            //auBuilder.RemoteVersion(@"C:\Users\n.tuytel\Resources\AutoUpdate\Sample\bin\Debug\net5.0\samples\sample1\version.json");



            var au = auBuilder.Build();
            #endregion


            // Update
            if (await au.UpdateAvailableAsync())
            {
                Console.WriteLine("AutoUpdate: found new version. Updating...");

                if (await au.Update())
                {
                    au.Restart();
                }
            }
            else
            {
                Console.WriteLine("AutoUpdate: no update found.");
            }

            // Publish
            if (await au.PublishAvailableAsync())
            {
                Console.WriteLine("AutoUpdate: has newer version. publishing...");

                await au.Publish();
            }
            else
            {
                Console.WriteLine("AutoUpdate: has no newer version.");
            }

            Console.ReadLine();
        }

    }

}
