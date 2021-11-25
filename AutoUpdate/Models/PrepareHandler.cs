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
                var filename = Path.GetFileNameWithoutExtension(name).ToLower();
                var ext = Path.GetExtension(name).ToLower();

                var type = "";
                var matchExt = ext.Contains("ps") || ext.Contains("bat") || ext.Contains("cmd") || ext.Contains("exe");
                if (filename.Contains(PRE_INSTALL)) type = "PRE";
                else if (filename.Contains(POST_INSTALL)) type = "POST";

                // skip invalid filenames
                if (type.Length == 0 || !matchExt) continue;

                // execute
                Console.WriteLine($"\n[Run {type}-INSTALL] {filename}{ext}");
                if (ext.Contains("ps"))
                {
                    // powershell script on command prompt (incl. bypass execution-policy)
                    filename = $"PowerShell.exe -command \"cd {FolderPath}; Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass; {filename}\"";
                }

                exitCode = ExecuteCommand(filename, hasTimeThreshold);
                if (exitCode != 0) break;
            }

            return exitCode;
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

        public static List<string> GetAllArguments(Func<List<string>> extraArguments = null)
        {
            //starts the (hopefully correcly updated) process using the original executable name startup arguments.
            var arguments = Environment.GetCommandLineArgs();

            //1st argument is always the executable path (see AppCore from MSDN  reference).
            var args = new List<string>();
            for (int i = 1; i < arguments.Length; i++)
            {
                args.Add(arguments[i]);
            }

            var extraArgs = extraArguments?.Invoke();
            if (extraArgs != null)
            {
                //keep it clean.
                foreach (var extraArg in extraArgs)
                {
                    if (!args.Contains(extraArg)) args.Add(extraArg);
                }
            }

            return args;
        }

    }
}
