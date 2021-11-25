using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;

namespace AutoUpdate.Provider
{
    public class JsonToVersionReader : IVersionReader
    {

        public Version GetVersion(string content)
        {
            try
            {
                VersionObject obj = JsonSerializer.Deserialize<VersionObject>(content);
                return new Version(obj.version);
            }
            catch (Exception)
            {
                return new Version(0, 0, 0, 0);
            }
        }


        [XmlRoot(ElementName = "root")]
        public class VersionObject
        {
            public VersionObject() { }              //def. CTor for serialization.

            public VersionObject(Version version)   //CTor to force consequent formatting of Version to string.
            {
                this.version = version.ToString();      //1.2.3.4
            }

            [XmlElement(ElementName = "version")]
            public string version { get; set; }
        }

    }

    

}
