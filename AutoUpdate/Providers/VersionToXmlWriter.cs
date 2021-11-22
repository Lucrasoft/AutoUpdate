﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;
using static AutoUpdate.Provider.JsonToVersionReader;

namespace AutoUpdate.Provider
{
    public class VersionToXmlWriter : IVersionWriter
    {
        public string SetVersion(Version version)
        {
            var obj = new JsonVersionObject(version);

            using var stringwriter = new System.IO.StringWriter();
            var serializer = new XmlSerializer(typeof(JsonVersionObject));
            serializer.Serialize(stringwriter, obj);

            return stringwriter.ToString();
        }

    }

}
