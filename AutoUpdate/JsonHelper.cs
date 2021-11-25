using Newtonsoft.Json;
using System.IO;

namespace AutoUpdate.Models
{
    public static class JsonHelper
    {
        public static T Read<T>(string filename) where T : new()
        {
            if (File.Exists(filename))
            {
                var txt = File.ReadAllText(filename);
                return JsonConvert.DeserializeObject<T>(txt);
            }

            return new T();
        }

        public static void Write<T>(T data, string filename) where T : new()
        {
            if (data == null) data = new T();
            string text = JsonConvert.SerializeObject(data);
            File.WriteAllText(filename, text);
        }
    }
}
