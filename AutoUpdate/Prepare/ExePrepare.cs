using AutoUpdate.Models;

namespace AutoUpdate
{
    public class ExePrepare : IPrepare
    {
        public string Extension { get; set; }

        public string GetCommand(string command)
        {
            return $"{command}.{Extension}";
        }
    }
}