using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate.Providers
{
    class FileVersionProvider : IVersionProvider
    {

        private string filename;
        private VersionFormat format;

        public FileVersionProvider(string filename, VersionFormat format = VersionFormat.AutoDetect)
        {
            this.filename = filename;
        }
  
        public Task<Version> GetVersionAsync()
        {
            var content = System.IO.File.ReadAllText(filename);

            if (format == VersionFormat.AutoDetect)
            {
                if (filename.EndsWith(".json")) 
                { 
                    format = VersionFormat.Json; 
                }
                else if (filename.EndsWith(".xml")) 
                { 
                    format = VersionFormat.Xml; 
                }
                else
                {
                    format = VersionFormat.Text;
                }
            }

            var reader = format.GetReader();
            var version = reader.GetVersion(content);

            return Task.FromResult(version);
            
        }

        public async Task SetVersionAsync(Version version)
        {
            throw new NotImplementedException();
        }
    }
}
