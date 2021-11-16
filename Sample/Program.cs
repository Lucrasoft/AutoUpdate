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

            var au = auBuilder.Build();
            #endregion

            if (await au.UpdateAvailableAsync(null))
            {
                Console.WriteLine("AutoUpdate: found new version. Updating...");

                await au.Update();
                au.Restart();
            }
            else
            {
                Console.WriteLine("AutoUpdate: no update found.");
            }

            Console.ReadLine();
        }

    }

}
