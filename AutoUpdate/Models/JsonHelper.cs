using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AutoUpdate.Models
{
    public class JsonHelper
    {
        public static (T, string) GetFile<T>(string path="./") where T : IHasFileName, new()
        {
            var model = new T();

            path = path.EndsWith("/") ? path[0..^1] : path;
            path = path.EndsWith("\\") ? path[0..^1] : path;
            return (model, $"{path}/{model.FileName}");
        }

        public static T Read<T>(string path="./") where T : IHasFileName, new()
        {
            (T newModel, string filename) = GetFile<T>(path);

            if (File.Exists(filename))
            {
                var txt = File.ReadAllText(filename);
                newModel = JsonSerializer.Deserialize<T>(txt);
            }

            return newModel;
        }

        public static void Write<T>(T data, string path="./") where T : IHasFileName, new()
        {
            (T newModel, string filename) = GetFile<T>(path);

            if (data == null) data = newModel;

            string text = JsonSerializer.Serialize(data);
            File.WriteAllText(filename, text);
        }
    }
}
