using System;
using System.Collections.Generic;
using System.Text;

namespace AutoUpdate.Provider
{
    interface IVersionWriter
    {
        string SetVersion(Version version);
    }
}
