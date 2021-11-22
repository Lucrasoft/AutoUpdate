using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using static AutoUpdate.Providers.JsonToVersionReader;

namespace AutoUpdate.Providers
{
    class XmlToVersionReader : IVersionReader
    {
        public Version GetVersion(string content)
        {
            try
            {
                var xmlSerializer = new XmlSerializer(typeof(JsonVersionObject));
                using var stringreader = new StringReader(content);
                var obj = (JsonVersionObject)xmlSerializer.Deserialize(stringreader);

                return new Version(obj.version);
            }
            catch (Exception)
            {
                return new Version(0, 0, 0, 0);
            }

        }

    }
}
