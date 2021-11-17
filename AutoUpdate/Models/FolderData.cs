using Microsoft.VisualStudio.Utilities;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AutoUpdate.Models
{
    public class FolderData : IHasFileName
    {
        // get static from generic supported in .NET 6
        [JsonIgnore]
        public string FileName => "structure_folder.json";

        [JsonProperty("current_filrnames")]
        public List<string> CurrentFileNames { get; set; } = new();

        [JsonProperty("previous_filenames")]
        public List<string> PreviousFileNames { get; set; } = new();
    }
}
