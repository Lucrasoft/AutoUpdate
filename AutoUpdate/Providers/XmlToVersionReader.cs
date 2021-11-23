using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using static AutoUpdate.Provider.JsonToVersionReader;

namespace AutoUpdate.Provider
{
    class XmlToVersionReader : IVersionReader
    {
        public Version GetVersion(string content)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(VersionObject));
                using var reader = new StringReader(content);
                var obj = (VersionObject)serializer.Deserialize(reader);

                return new Version(obj.version);
            }
            catch (Exception)
            {
                return new Version(0, 0, 0, 0);
            }
        }

    }
}
