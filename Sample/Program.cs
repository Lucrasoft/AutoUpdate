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

            if (args.Length == 1)
            {
                auBuilder.AddGithub(url: args[0]);
            }
            else
            {
                auBuilder.AddBlobStorage(connectionString: args[0], container: args[1]);
            }


            //auBuilder.RemoteVersion(new Uri("https://teststorage777.blob.core.windows.net/releases/version.json"));

            var au = auBuilder.Build();
            #endregion





            // UPDATE
            if (await au.UpdateAvailableAsync())
            {
                Console.WriteLine("AutoUpdate: found new version. Updating...");

                if(await au.Update())
                {
                    au.Restart();
                }
            }
            else
            {
                Console.WriteLine("AutoUpdate: no update found.");
            }


            // PUBLISH
            //await auPublish(verbose:true);

            Console.ReadLine();
        }

    }

}
