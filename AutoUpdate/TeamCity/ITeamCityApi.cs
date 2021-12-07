using System;

namespace AutoUpdate.TeamCity
{
    public interface ITeamCityApi
    {
        /// <summary>
        /// Get latest package from teamcity.
        /// </summary>
        byte[] GetLatestPackage();

        /// <summary>
        /// Get Latest version from teamcity on given TypeName id.
        /// </summary>
        /// <returns></returns>
        Version GetLatestVersion();
    }
}