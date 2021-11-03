using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate
{
    public interface IVersionProvider
    {
        Task<Version> GetVersionAsync();

    }
}
