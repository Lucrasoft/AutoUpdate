using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    public class VersionFile
    {
        [JsonProperty("latest")]
        public string Latest { get; set; }
    }
}
