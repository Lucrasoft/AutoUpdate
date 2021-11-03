using System;
using System.Collections.Generic;
using System.Text;

namespace AutoUpdate.Providers
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
    }

}
