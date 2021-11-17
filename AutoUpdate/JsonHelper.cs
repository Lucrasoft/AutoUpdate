using Newtonsoft.Json;
using System.IO;

namespace AutoUpdate.Models
{
    public static class JsonHelper
    {
        public static (T, string) GetFile<T>(string path="./") where T : IHasFileName, new()
        {
            var model = new T();
            var filename = "";

            if(path.EndsWith("/"))
            {
                filename = $"{path[0..^1]}/{model.FileName}";
            }
            else if (path.EndsWith("\\"))
            {
                filename = $"{path[0..^1]}\\{model.FileName}";
            }
            else
            {
                path += path.Contains('\\') ? '\\' : '/';
                filename = $"{path}{model.FileName}";
            }

            return (model, filename);
        }

        public static T Read<T>(string path="./") where T : IHasFileName, new()
        {
            (T newModel, string filename) = GetFile<T>(path);

            if (File.Exists(filename))
            {
                var txt = File.ReadAllText(filename);
                newModel = JsonConvert.DeserializeObject<T>(txt);
            }

            return newModel;
        }

        public static void Write<T>(T data, string path="./") where T : IHasFileName, new()
        {
            (T newModel, string filename) = GetFile<T>(path);

            if (data == null) data = newModel;

            string text = JsonConvert.SerializeObject(data);
            File.WriteAllText(filename, text);
        }
    }
}
