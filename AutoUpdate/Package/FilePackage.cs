﻿using AutoUpdate.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate.Package
{
    class FilePackage : IPackage
    {
        private readonly string filename;
        private Func<Version, string> filenameFunc;

        public FilePackage(string filename)
        {
            this.filename = filename;
        }

        public FilePackage(Func<Version, string> filenameFunc)
        {
            this.filenameFunc = filenameFunc;
        }

        public Task<byte[]> GetContentAsync(Version version, EventHandler<ProgressDownloadEvent> handler)
        {
            string fname = filename; 
            if (filenameFunc!=null)
            {
                fname = filenameFunc(version);
            }

            //var bytes = System.IO.File.ReadAllBytes(fname);
            //return Task.FromResult(bytes);
            using (var fsFile = System.IO.File.OpenRead(fname))
            {
                System.IO.MemoryStream stream = PackageUtils.FillFromRemoteStream(
                    fsFile, 
                    fsFile.Length, 
                    handler, 
                    "reading"
                );

                return Task.FromResult(stream.ToArray());
            }
        }

        public Task SetContentAsync(byte[] data, Version version, EventHandler<ProgressUploadEvent> handler)
        {

            return Task.CompletedTask;
        }
    }
}
