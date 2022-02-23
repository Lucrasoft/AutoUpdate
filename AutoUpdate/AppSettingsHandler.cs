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
            JObject currFile = null;
            string filename = null;

            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                // filter filename 
                if (!entry.FullName.EndsWith("appsettings.json")) continue;
                filename = entry.FullName;

                // read JSON current file
                using var sr = new StreamReader(entry.Open());
                using var r = new JsonTextReader(sr);
                currFile = (JObject)JToken.ReadFrom(r);

                var prevFilename = $"{_prevFolderPath}/{filename}";
                if (!File.Exists(prevFilename))
                {
                    return (filename, currFile);
                }

                using var file2 = File.OpenText(prevFilename);
                using var reader2 = new JsonTextReader(file2);
                var prevFile = (JObject)JToken.ReadFrom(reader2);

                // update NEW content with OLD data
                bool beenCalled = false;
                foreach (var x1 in currFile)
                {
                    foreach (var x2 in prevFile)
                    {
                        // keep old existing objects
                        if (x1.Key == x2.Key)
                        {
                            currFile[x1.Key] = x2.Value;
                            beenCalled = true;
                            break;
                        }
                    }
                }

                // skip rest of files
                if (beenCalled) break;
            }

            return (filename, currFile);
        }

        public void SetFile(string filename, JObject data)
        {
            File.WriteAllText(filename, data.ToString());
        }
    
    }
}
