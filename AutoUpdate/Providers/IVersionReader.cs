using System;
using System.Collections.Generic;
using System.Text;

namespace AutoUpdate.Provider
{
    interface IVersionReader
    {
        Version GetVersion(string content);
    }
}
