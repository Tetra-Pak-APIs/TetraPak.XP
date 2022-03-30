using System;

namespace TetraPak.XP.StringValues;

/// <summary>
///   Provides convenient extensions for working with <see cref="VersionPattern"/>. 
/// </summary>
public static class VersionPatternHelper
{
    /// <summary>
    ///   Adjusts a <see cref="Version"/> based on the <see cref="VersionPattern"/> value. 
    /// </summary>
    /// <param name="self">
    ///   The pattern used to adjust the <paramref name="version"/>.
    /// </param>
    /// <param name="version">
    ///   The <see cref="Version"/> to be adjusted.
    /// </param>
    /// <param name="policy">
    ///   Specifies a policy to be applied for adjusting the <paramref name="version"/>. 
    /// </param>
    /// <returns>
    ///   A new adjusted <see cref="Version"/>. 
    /// </returns>
    public static Version Adjust(this VersionPattern self, Version? version, VersioningPolicy policy = VersioningPolicy.Soft)
    {
        var major = self.IsPattern ? self.Major + version?.Major ?? 0 : self.Major;
        var minor = self.IsPattern ? self.Minor + version?.Minor ?? 0 : self.Minor;
        var revision = self.IsPattern ? self.Revision + version?.Revision ?? 0 : self.Revision;
        var build = self.IsPattern ?  self.Build + version?.Build ?? 0 : self.Build; 
        var newVersion = new Version(major, minor, build, revision);
        return newVersion.Adjust(version, policy);
    }
}