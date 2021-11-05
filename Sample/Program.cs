using AutoUpdate;
using System;
using System.Threading.Tasks;

namespace Sample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var au = new AutoUpdateBuilder()
                .AddGithubRelease(new Uri("https://github.com/niektuytel/HelloRelease"))
                .Build();

            if (await au.UpdateAvailableAsync(null))
            {
                Console.WriteLine("AutoUpdate: found new version. Updating...");
                await au.Update(null);
                //au.Restart(null);
            }
            else
            {
                Console.WriteLine("AutoUpdate: no update found.");
            }


        }


    }


}
