using Microsoft.Extensions.Logging;
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

        public (string, JObject) GetFromArchive(ZipArchive archive, IEnumerable<string> filenames, ILogger logger)
        {
            JObject currFile = null;
            string filename = null;

            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                // filter filename 
                filename = entry.FullName;
                var matches = filenames.Where(fname => filename.EndsWith(fname));
                if (!matches.Any()) 
                {
                    continue;
                }

                // read JSON current file
                using var sr = new StreamReader(entry.Open());
                using var r = new JsonTextReader(sr);
                currFile = (JObject)JToken.ReadFrom(r);

                var prevFilename = $"{_prevFolderPath}/{filename}";
                if (!File.Exists(prevFilename))
                {
                    logger.LogCritical($"{prevFilename} is not been founded");
                    return (filename, currFile);
                }

                using var file2 = File.OpenText(prevFilename);
                using var reader2 = new JsonTextReader(file2);
                var prevFile = (JObject)JToken.ReadFrom(reader2);

                // update NEW content with OLD data
                foreach (var x1 in currFile)
                {
                    foreach (var x2 in prevFile)
                    {
                        // keep old existing objects
                        if (x1.Key == x2.Key)
                        {
                            currFile[x1.Key] = x2.Value;
                            break;
                        }
                    }
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
