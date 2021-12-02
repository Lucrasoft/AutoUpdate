using AutoUpdate.Models;
using AutoUpdate.Prepare;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate
{
    public class PrepareHandler
    {
        private const string PRE_INSTALL = "pre-install";
        private const string POST_INSTALL = "post-install";
        private const int ERROR_TIME_THRESHOLD = 60000;
        private const int ERROR_TIME_EXITCODE = 1337;

        private static readonly List<PrepareItem> PrepareItems = new()
        {
            new PrepareItem("ps1", (path) => new PsPrepare(path)),
            new PrepareItem("bat", (path) => new BatPrepare()),
            new PrepareItem("cmd", (path) => new CmdPrepare()),
            new PrepareItem("exe", (path) => new ExePrepare()),
        };

        public string FolderPath { get; private set; }

        public PrepareHandler(string path)
        {
            FolderPath = path;
        }

        public int RunPreAndPostInstall(bool hasTimeThreshold=true)
        {
            int exitCode = 0;

            foreach (var name in Directory.GetFiles(FolderPath))
            {
                // filter untill valid filename
                (var filename, var ext) = GetFilenameAndExtention(name);
                if (ext == null) continue;

                var installName = FilenameContainsString(filename);
                if (installName == null) continue;

                var prepare = GetPrepare(ext);
                if (prepare == null)
                {
                    throw new ArgumentNullException($"Missing extention Attribute named: {ext}");
                }

                // get specific filename
                var command = prepare.GetCommand(filename);

                // run pre/build script before download
                Console.WriteLine($"\n[Run script: {installName}] cmd:{command}");
                exitCode = ExecuteCommand(command, hasTimeThreshold);
                if (exitCode != 0) return exitCode;
            }

            return exitCode;
        }

        private static (string, string) GetFilenameAndExtention(string name)
        {
            var filename = Path.GetFileNameWithoutExtension(name).ToLower();
            var ext = Path.GetExtension(name).ToLower().Split(".")[^1];
            return (filename, ext);
        }

        private static string FilenameContainsString(string filename)
        {
            filename = filename.ToLower();

            if (filename.Contains(PRE_INSTALL))
            {
                return PRE_INSTALL;
            }
            else if (filename.Contains(POST_INSTALL))
            {
                return POST_INSTALL;
            }

            return null;
        }

        private static int ExecuteCommand(string command, bool hasTimeThreshold)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            var process = Process.Start(processInfo);
            if(hasTimeThreshold)
            {
                if(!process.WaitForExit(ERROR_TIME_THRESHOLD))
                {
                    Console.WriteLine($"error >> Execution takes to long exception called.");
                    Console.WriteLine($"exitcode >> {ERROR_TIME_EXITCODE}");
                    return ERROR_TIME_EXITCODE;
                }
            }
            else
            {
                process.WaitForExit();
            }

            // *** Read the streams ***
            // Warning: This approach can lead to deadlocks, see Edit #2
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            var exitCode = process.ExitCode;

            // log responses
            if (!string.IsNullOrEmpty(output)) Console.Write($"output >> {output}");
            if (!string.IsNullOrEmpty(error)) Console.Write($"error >> {error}");
            Console.WriteLine($"exitcode >> {exitCode}\n");

            process.Close();
            return exitCode;
        }

        private IPrepare GetPrepare(string ext)
        {
            ext = ext.ToLower();

            foreach (var item in PrepareItems)
            {
                var ext_item = item.Extension.ToLower();
                if (ext.Contains(ext_item))
                {
                    return item.Create(FolderPath);
                }
            }

            return null;
        }

    }
}
