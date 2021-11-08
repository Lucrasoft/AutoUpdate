using AutoUpdate.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate.Package
{
    public interface IPackage
    {

        Task<byte[]> GetContentAsync(Version version, EventHandler<DownloadProgressEventArgs> handler);

    }
}
