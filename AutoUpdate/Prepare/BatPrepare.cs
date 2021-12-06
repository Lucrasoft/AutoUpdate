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
        public string Extension { get; set; }

        public string GetCommand(string command)
        {
            return $"{command}.{Extension}";
        }
    }
}
