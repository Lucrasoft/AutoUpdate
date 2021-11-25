using System;

namespace AutoUpdate.Provider
{
    public class VersionToStringWriter : IVersionWriter
    {
        public string SetVersion(Version version)
        {
            return PackageUtils.GetVersionString(version);
        }

    }

}
