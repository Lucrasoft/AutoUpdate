using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate.Models
{
    public class BatPrepare : IPrepare
    {
        public string Extention => "bat";

        public int Execute(string command)
        {
            return 0;
            //if (filename == PRE_INSTALL)
            //{
            //    type = "PRE";
            //}
            //else if (filename == PRE_INSTALL)
            //{
            //    type = "POST";
            //}



            //foreach (var name in Directory.GetFiles(FolderPath))
            //{
            //    var filename = Path.GetFileNameWithoutExtension(name).ToLower();
            //    var ext = Path.GetExtension(filename).ToLower();

            //    var type = filename.Split("-")[0].ToUpper();
            //    var matchExt = ext.Contains("ps") || ext.Contains("bat") || ext.Contains("cmd") || ext.Contains("exe");

            //    if (filename == PRE_INSTALL)
            //    {
            //        type = "PRE";
            //    }
            //    else if (filename == PRE_INSTALL)
            //    {
            //        type = "POST";
            //    }

            //    // skip invalid filenames
            //    if (type.Length == 0 || !matchExt) continue;

            //    // execute
            //    Console.WriteLine($"[Run {type}-INSTALL] {filename}");
            //    if (ext.Contains("ps"))
            //    {
            //        // powershell script on command prompt (incl. bypass execution-policy)
            //        filename = $"PowerShell.exe -command \"cd {FolderPath}; Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass; {filename}\"";
            //    }

            //    ExecuteCommand(filename);
            //}
            //throw new NotImplementedException();
        }
    }
}
