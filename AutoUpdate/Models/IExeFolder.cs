using System.IO.Compression;

namespace AutoUpdate.Models
{
    /// <summary>
    /// Executable folder where Auto Updates happends 
    /// </summary>
    public interface IExeFolder
    {
        /// <summary>
        /// Current running application file.
        /// </summary>
        string ExeFile { get; }

        /// <summary>
        /// Current folder directory
        /// </summary>
        string ExePath { get; }

        /// <summary>
        /// History storage executable folder.
        /// </summary>
        VersionsData FolderData { get; }

        /// <summary>
        /// Kill the process of previous application version.
        /// Remove all deprecrated files.
        /// </summary>
        void RemoveDuplicatedFileNames();

        /// <summary>
        /// Get latest version installed executable filename
        /// </summary>
        string GetExecutableFileName();


        /// <summary>
        /// Zip folder into memory bytes
        /// </summary>
        /// <returns>zipped folder</returns>
        public byte[] Zip();

        /// <summary>
        /// Zip this folder into given filename
        /// </summary>
        /// <param name="filename">zip filename</param>
        void ZipTo(string filename);
    }
}