using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace AutoUpdate.Models
{
    public class ExeFolder : IExeFolder
    {
        public string ExeFile { get; private set; }
        public string ExePath { get; private set; }
        public ExeFolderData FolderData { get; private set; }

        public ExeFolder()
        {
            //Huidige locatie van de .exe file bepaald de bestemming
            ExeFile = Process.GetCurrentProcess().MainModule.FileName;
            ExePath = Path.GetDirectoryName(ExeFile);
            FolderData = JsonHelper.Read<ExeFolderData>(ExePath);
        }

        public void RemoveDuplicatedFileNames()
        {
            var files = Directory.GetFiles(ExePath);
            var currFileNames = FolderData.CurrentFileNames;

            // update filenames
            if (currFileNames.Count > 0 && !currFileNames.SequenceEqual(files))
            {
                var duplicated = files.Except(currFileNames).ToList();
                if (duplicated.Count > 0)
                {
                    // kill previous running program file
                    KillProcesses(duplicated);

                    foreach (var filename in duplicated)
                    {
                        try
                        {
                            File.Delete(filename);
                            Console.WriteLine($"[DELETE] {filename}");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"[DELETE FAILED] {filename}\n\t --> Exception Message: {e.Message}");
                        }
                    }
                }
            }
        }

        public void UpdateFileNames(ZipArchive archive)
        {
            var prevFileNames = Directory.GetFiles(ExePath).ToList();
            var currFileNames = archive.Entries.Select(a => $"{ExePath}\\{a.FullName}").ToList();

            // set all history filenames
            (_, var jsonFile) = JsonHelper.GetFile<ExeFolderData>(path: ExePath);
            currFileNames.Add(jsonFile);
            prevFileNames.Add(jsonFile);

            FolderData.CurrentFileNames = currFileNames.ToList();
            FolderData.PreviousFileNames = prevFileNames.ToList();
            JsonHelper.Write(FolderData, path: ExePath);
        }

        public string GetExecutableFileName()
        {
            var files = FolderData.CurrentFileNames;
            var exes = files.Where(a => a.EndsWith(".exe")).ToList();

            if (exes.Count > 1 || !exes.Any())
            {
                var docs = files.Except(FolderData.PreviousFileNames).ToList();
                exes = docs.Where(a => a.EndsWith(".exe")).ToList();
                var file = exes.Count == 0 ? ExeFile : exes.First();

                Console.WriteLine($"[WARNING] There are more .exe Files, take file: {file}\n");

                return ExeFile;
            }

            return exes.First();
        }

        /// <summary>
        /// Kill process of deprecated files.
        /// </summary>
        /// <param name="deprecated">File names that are not been used.</param>
        private static void KillProcesses(List<string> deprecated)
        {
            // kill previous running program
            var exeFiles = deprecated.Where(a => a.EndsWith(".exe"));
            foreach (var exe in exeFiles)
            {
                foreach (var proc in Process.GetProcessesByName(exe))
                {
                    proc.Kill();
                }
            }
        }

    }
}
