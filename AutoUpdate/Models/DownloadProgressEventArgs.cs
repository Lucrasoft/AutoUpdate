using System;

namespace AutoUpdate.Models
{
    public class DownloadProgressEventArgs: EventArgs
    {
        public DownloadProgressEventArgs(string description, int procentage)
        {
            Description = description;
            Procentage = procentage;
        }

        public string Description { get; set; }

        public int Procentage { get; set; }
    }


}
