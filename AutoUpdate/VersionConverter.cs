using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate.TeamCity
{
    public class VersionConverter
    {
        /// <summary>
        /// Lucrasoft has for versioning the following structure:
        /// {major}.{minor}.{patch}-suffix.{build}
        /// By using System.Version we need to convert it to a structure:
        /// {major}.{minor}.{patch}.{build}
        /// </summary>
        private static Version LucrasoftVersionToVersion(string version)
        {
            if(!version.Contains("."))
            {
                throw new InvalidCastException($"version:{version} should contain `.` for version convertion");
            }

            var parts = version.Split(".");
            if (parts.Length != 4)
            {
                throw new InvalidCastException($"version:{version} should contain as least 2 `.` for version convertion");
            }

            var line = parts[2].Split("-");
            if (line.Length != 2)
            {
                throw new InvalidCastException($"Lucrasoft version contains patch-suffix in version.");
            }

            var major = int.Parse(parts[0]);
            var minor = int.Parse(parts[1]);
            var patch = int.Parse(line[0]);// suffix = line[1];
            var build = int.Parse(parts[3]);

            return new Version(major, minor, patch, build);
        }

        /// <summary>
        /// Convert string into System.Version()
        /// </summary>
        /// <param name="version">value that will been converted</param>
        public static Version Getversion(string version)
        {
            try
            {
                return new Version(version);
            }
            catch (Exception) { }

            try
            {
                return LucrasoftVersionToVersion(version);
            }
            catch (Exception) { }

            throw new ArgumentException($"Readed version: {version} into Version() failed.");
        }
    }
}
