using AutoUpdate;
using AutoUpdate.Models;
using System;
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

            //if (args.Length == 1)
            //{
            //    auBuilder.AddGithub(url: args[0]);
            //}
            //else
            //{
            //    auBuilder.AddBlobStorage(connectionString: args[0], container: args[1]);
            //}

            auBuilder.AddPackage(@"C:\Users\n.tuytel\Resources\AutoUpdate\Sample\bin\Debug\1.0.38.0.zip");
            auBuilder.RemoteVersion(@"C:\Users\n.tuytel\Resources\AutoUpdate\Sample\bin\Debug\version.json");

            var au = auBuilder.Build();
            #endregion


            //// Update
            //if (await au.UpdateAvailableAsync())
            //{
            //    Console.WriteLine("AutoUpdate: found new version. Updating...");

            //    if(await au.Update())
            //    {
            //        au.Restart();
            //    }
            //}
            //else
            //{
            //    Console.WriteLine("AutoUpdate: no update found.");
            //}



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
