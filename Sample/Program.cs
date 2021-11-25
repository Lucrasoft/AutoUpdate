using AutoUpdate;
using AutoUpdate.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace OtherNamedConsole
{
    class Program
    {

        static async Task<int> Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            #region generate AutoUpdate (output: au)
            var auBuilder = new AutoUpdateBuilder();

            // auBuilder.SetHttpClient(new HttpClient());

             auBuilder.SetPackageUpdateType(PackageUpdateEnum.SideBySide);

            auBuilder.AddBlobStorage(
                connectionString: "XXXX",
                container: "XXXX"
            );

            //auBuilder.AddGithub("https://github.com/niektuytel/HelloRelease");

            //auBuilder.AddPackage(@"C:\Users\n.tuytel\Resources\AutoUpdate\Sample\bin\Debug\1.0.38.0.zip");
            ////auBuilder.LocalVersion(@"C:\Users\n.tuytel\Resources\AutoUpdate\Sample\bin\Debug\net5.0\samples\sample1\version.xml");
            ////auBuilder.LocalVersion(@"C:\Users\n.tuytel\Resources\AutoUpdate\Sample\bin\Debug\net5.0\samples\sample1\version.txt");
            //auBuilder.LocalVersion(@"C:\Users\n.tuytel\Resources\AutoUpdate\Sample\bin\Debug\net5.0\samples\sample1\version.json");
            //auBuilder.RemoteVersion(@"C:\Users\n.tuytel\Resources\AutoUpdate\Sample\bin\Debug\net5.0\samples\sample1\version.xml");
            ////auBuilder.RemoteVersion(@"C:\Users\n.tuytel\Resources\AutoUpdate\Sample\bin\Debug\net5.0\samples\sample1\version.txt");
            ////auBuilder.RemoteVersion(@"C:\Users\n.tuytel\Resources\AutoUpdate\Sample\bin\Debug\net5.0\samples\sample1\version.json");



            var au = auBuilder.Build();
            #endregion


            // Update
            if (await au.UpdateAvailableAsync())
            {
                Console.WriteLine("AutoUpdate: found new version. Updating...");

                if (await au.Update())
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
