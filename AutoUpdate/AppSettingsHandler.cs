using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate
{
    internal class AppSettingsHandler
    {
        private readonly string _prevFolderPath;

        public AppSettingsHandler(string folderPath)
        {
            _prevFolderPath = folderPath;
        }

        public (string, JObject) GetFromArchive(ZipArchive archive)
        {
            JObject prevFile = null;
            JObject currFile = null;
            string filename = null;

            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                // filter filename 
                if (!entry.FullName.EndsWith("appsettings.json")) continue;
                filename = entry.FullName;
                var prevFilename = $"{_prevFolderPath}/{filename}";

                // read JSON current file
                using var sr = new StreamReader(entry.Open());
                using var r = new JsonTextReader(sr);
                currFile = (JObject)JToken.ReadFrom(r);

                // read JSON prev file
                if (!File.Exists(prevFilename))
                {
                    // has no prev file
                    return (filename, currFile);
                }
                else
                {
                    using var file = File.OpenText(prevFilename);
                    using var reader = new JsonTextReader(file);
                    prevFile = (JObject)JToken.ReadFrom(reader);

                    // update NEW content with OLD data
                    foreach (var x1 in currFile)
                    {
                        string name1 = x1.Key;
                        JToken value1 = x1.Value;

                        foreach (var x2 in prevFile)
                        {
                            string name2 = x2.Key;
                            JToken value2 = x2.Value;

                            // keep old existing objects
                            if (name1 == name2)
                            {
                                value1 = value2;
                                break;
                            }
                        }
                    }

                    break;
                }
            }

            return (filename, currFile);
        }

        public void SetFile(string filename, JObject data)
        {
            File.WriteAllText(filename, data.ToString());
        }



    }
}
