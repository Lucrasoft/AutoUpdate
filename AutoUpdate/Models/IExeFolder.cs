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
        ExeFolderData FolderData { get; }

        /// <summary>
        /// Kill the process of previous application version.
        /// Remove all deprecrated files.
        /// </summary>
        void RemoveDuplicatedFileNames();

        /// <summary>
        /// Update <see cref="ExeFolderData"/>.<br/>
        /// On file named: <see cref="ExeFolderData.FileName"/>
        /// </summary>
        /// <param name="archive">New unzipped application version</param>
        void UpdateFileNames(ZipArchive archive);

        /// <summary>
        /// Get latest version installed executable filename
        /// </summary>
        string GetExecutableFileName();
    }
}