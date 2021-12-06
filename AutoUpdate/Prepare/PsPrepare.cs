using AutoUpdate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate.Prepare
{
    public class PsPrepare: IPrepare
    {
        public string Extension { get; set; }

        public string PathLocation { get; set; }

        public PsPrepare(string pathLocation)
        {
            PathLocation = pathLocation;
        }

        public string GetCommand(string filename)
        {
            // powershell script on command prompt (incl. bypass execution-policy)
            return $"PowerShell.exe -command \"cd {PathLocation}; Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass; .\\{filename}.{Extension}\"";
        }

    }
}
