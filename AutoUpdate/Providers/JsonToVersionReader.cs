using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace AutoUpdate.Providers
{
    public class JsonToVersionReader : IVersionReader
    {

        public Version GetVersion(string content)
        {
            try
            {
                JsonVersionObject obj = JsonSerializer.Deserialize<JsonVersionObject>(content);
                return new Version(obj.version);
            }
            catch (Exception ex)
            {
                return new Version(0, 0, 0, 0);
            }
        }


        public class JsonVersionObject
        {
            public JsonVersionObject() { }              //def. CTor for serialization.

            public JsonVersionObject(Version version)   //CTor to force consequent formatting of Version to string.
            {
                this.version = version.ToString();      //1.2.3.4
            }

            public string version { get; set; }
        }

    }

    

}
