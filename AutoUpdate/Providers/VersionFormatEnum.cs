using System;
using System.Collections.Generic;
using System.Text;

namespace AutoUpdate.Provider
{
    public enum VersionFormat
    {
        AutoDetect = 0,
        Text= 1,
        Json =2,
        Xml =3
    }

    static class VersionFormatExtension
    {
        public static IVersionReader GetReader(this VersionFormat format)
        {
            switch (format)
            {
                case VersionFormat.Json:
                    return new JsonToVersionReader();
                case VersionFormat.Text:
                    return new StringToVersionReader();
                case VersionFormat.Xml:
                    return new XmlToVersionReader();
                default:
                    throw new ArgumentOutOfRangeException("This format does not have a versionreader.");
            }
        }

        public static IVersionWriter GetWriter(this VersionFormat format)
        {
            switch (format)
            {
                case VersionFormat.Json:
                    return new VersionToJsonWriter();
                case VersionFormat.Text:
                    return new VersionToStringWriter();
                case VersionFormat.Xml:
                    return new VersionToXmlWriter();
                default:
                    throw new ArgumentOutOfRangeException("This format does not have a versionreader.");
            }
        }

    }

}
