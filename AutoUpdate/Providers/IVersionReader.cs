using System;
using System.Collections.Generic;
using System.Text;

namespace AutoUpdate.Providers
{
    interface IVersionReader
    {
        Version GetVersion(string content);
    }
}
