using AutoUpdate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate.Prepare
{
    public class CmdPrepare : IPrepare
    {

        public string Extension { get; set; }

        public string GetCommand(string command)
        {
            return $"{command}.{Extension}";
        }
    }
}
