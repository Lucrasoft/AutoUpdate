using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using static AutoUpdate.Provider.JsonToVersionReader;

namespace AutoUpdate.Provider
{
    public class VersionToJsonWriter : IVersionWriter
    {
        public string SetVersion(Version version)
        {
            var obj = new VersionObject(version);
            return JsonSerializer.Serialize(obj);
        }

    }

}
