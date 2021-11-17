using Microsoft.VisualStudio.Utilities;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AutoUpdate.Models
{
    public class FolderData : IHasFileName
    {
        // get static from Type is 
        public string FileName => "structure_folder.json";

        [JsonProperty("latest_structure")]
        public List<string> CurrentFileNames = new();

        [JsonProperty("previous_structure")]
        public List<string> PreviousFileNames = new();
    }
}
