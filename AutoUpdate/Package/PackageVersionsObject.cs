using Microsoft.VisualStudio.Utilities;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AutoUpdate.Package
{
    public class PackageVersionsObject
    {
        [JsonProperty("versions")]
        public List<string> Versions { get; set; } = new();

        [JsonProperty("failed_versions")]
        public List<string> FailedVersions { get; set; } = new();
    }
}
