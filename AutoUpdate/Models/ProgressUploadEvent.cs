using System;

namespace AutoUpdate.Models
{
    public class ProgressUploadEvent: ProgressDownloadEvent
    {
        public ProgressUploadEvent(string description, int procentage): base(description, procentage)
        {}
    }
}
