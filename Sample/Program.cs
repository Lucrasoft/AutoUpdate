using AutoUpdate;
using System;
using System.Threading.Tasks;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var au = new AutoUpdateBuilder()
                .AddGithubRelease(new Uri("https://github.com/niektuytel/HelloRelease"))
                .Build();

            var updatable = au.UpdateAvailableAsync(null);
            updatable.Wait();

            if (updatable.Result)
            {
                
            }




        }
    }
}
