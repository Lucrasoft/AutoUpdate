using System;
using System.Collections.Generic;
using System.Text;


namespace AutoUpdate.Provider
{
    public class StringToVersionReader : IVersionReader
    {
        public Version GetVersion(string content)
        {
            //Content kan in meest rare vormen aangeboden worden. Zoveel mogelijk 'oke' parsen? 
            //Eigenlijk is zoveel mogelijk parse-en een slecht pattern toch? Denk denk denk..

            //Step 1. Is this multiline content ? -> we use the first not-empty line.
            var lines = content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (lines.Length > 0) content = lines[0]; else content = string.Empty;

            //Step 2. Split on point and proces the parts until a conversion fails.
            var parts = content.Split(".");
            var cont = true;
            int counter = 0;
            var v = new int[4];
            
            while (cont)
            {
                if (!int.TryParse(parts[counter], out int val))
                {
                    cont = false;
                };
                v[counter] = val;
                counter++;
                if (counter>3) { cont = false; } //all 4 version components read.
                if (counter>=parts.Length) { cont = false; } //IndexOutOfRange protection.
            }

            return new Version(v[0], v[1], v[2], v[3]);
        }
    }
}
