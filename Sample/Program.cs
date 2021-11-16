﻿using AutoUpdate;
using AutoUpdate.Models;
using System;
using System.Threading.Tasks;

namespace Sample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            //var connectionString = "DefaultEndpointsProtocol=https;AccountName=hellorelease;AccountKey=BMMilJYqCibCKFR7P9iZ9x7A8TbpiSZ2mrJdu8EgoyiPUuhDjS1i+T0q1Y2tocJMZODD9qbrX9BBDkoN9ureaw==;EndpointSuffix=core.windows.net";
            //var container = "containername";
            //var blob = new BlobStorage(connectionString, container);

            string connectionString = "DefaultEndpointsProtocol=https;AccountName=teststorage777;AccountKey=Tq56DDVRLkmY6S/srcXoGsas6n1ao4fVeYYLdamWvR+Mxih4LZ6H2B3IBH40xv8AUGaAvOidcA+x6CcM9H5hrw==;EndpointSuffix=core.windows.net";
            string container = "releases";


            var au = new AutoUpdateBuilder()
                .AddBlobStorage(connectionString, container)
                //.AddGithub("https://github.com/niektuytel/HelloRelease")
                .Build();

            //au.OnDownloadProgress += Au_OnDownloadProgress;

            if (await au.UpdateAvailableAsync())
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

        //private static void Au_OnDownloadProgress(object sender, DownloadProgressEventArgs e)
        //{
        //    Console.WriteLine($"Description: {e.Description}");
        //    Console.WriteLine($"Progress: {e.Procentage}");
        //    Console.WriteLine($"finish");
        //}

    }
}
