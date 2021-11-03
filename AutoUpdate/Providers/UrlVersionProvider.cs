using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate.Providers
{
    class UrlVersionProvider : IVersionProvider
    {

        private readonly Uri remoteurl;


        //Should be static! 
        private static HttpClient client = new HttpClient();


        public UrlVersionProvider(Uri url)
        {
            this.remoteurl = url;
        }

        public async Task<Version> GetVersionAsync()
        {
            
            var response = await client.GetAsync(remoteurl);
            if (!response.IsSuccessStatusCode)
            {
                return new Version(0, 0, 0, 0);
            }
            var content = await response.Content.ReadAsStringAsync();

            //TODO -> content-type geeft wel goed beeld wat er ongeveer terug komt (XML of JSON of Text/Raw)
            //Example var contenttype = x.Content.Headers.ContentType;
            //
            //voor nu doen we JSON assumption + 
            //expected json format is { .... , "version" : "x.y.z" , ..... } 

            var reader = new JsonToVersionReader();
            var version = reader.GetVersion(content);

            return version;

        }

    }
}
