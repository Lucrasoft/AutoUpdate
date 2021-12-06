using AutoUpdate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate.Prepare
{
    public class PrepareItem
    {
        public readonly string Extension;

        private readonly Func<string, IPrepare> _handler;

        public PrepareItem(string extention, Func<string, IPrepare> handler)
        {
            Extension = extention;
            _handler = handler;
        }

        public IPrepare Create(string folderPath)
        {
            var prepare = _handler(folderPath);
            prepare.Extension = Extension;

            return prepare;
        }
    }
}
