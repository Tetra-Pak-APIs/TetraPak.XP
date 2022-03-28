using System;

namespace TetraPak.XP.Nuget
{
public static class NugetVersionHelper
    {
        public static Version ToVersion(this NugetVersion nugetVersion)
        {
            return nugetVersion.Prerelease is { }
                ? new Version(
                    nugetVersion.Major,
                    nugetVersion.Minor,
                    nugetVersion.Revision,
                    nugetVersion.Prerelease.Version)
                : new Version(nugetVersion.Major,
                    nugetVersion.Minor,
                    nugetVersion.Revision);
        }
        
        public static VersionPattern ToVersionPattern(this NugetVersion nugetVersion)
        {
            var versionPattern = nugetVersion.Prerelease is { }
                ? new VersionPattern(
                    nugetVersion.Major,
                    nugetVersion.Minor,
                    nugetVersion.Revision,
                    nugetVersion.Prerelease.Version)
                : new VersionPattern(
                    nugetVersion.Major,
                    nugetVersion.Minor,
                    nugetVersion.Revision,
                    0);
            
            versionPattern.IsPattern = nugetVersion.IsPattern;
            return versionPattern;
        }
        
        public static NugetVersion Adjust(this NugetVersion self, NugetVersion targetVersion, VersioningPolicy policy = VersioningPolicy.Soft)
        {
            if (self.IsPattern)
                return adjustWithPattern();

            // if (self >= targetVersion)
            //     return self;

            return policy switch
            {
                VersioningPolicy.Soft => targetVersion >= self ? targetVersion : self,
                VersioningPolicy.Hard => self,
                _ => throw new ArgumentOutOfRangeException(nameof(policy), policy, null)
            };
            
            NugetVersion adjustWithPattern()
            {
                var major = self.Major + targetVersion.Major;
                var minor = self.Minor + targetVersion.Minor;
                var revision = self.Revision + targetVersion.Revision;
                var prerelease = self.IsPrerelease ? self.Prerelease!.Adjust(targetVersion.Prerelease, policy) : null; 
                var newVersion = prerelease is {} 
                    ? new NugetVersion($"{major}.{minor}.{revision}-{prerelease}") 
                    : new NugetVersion($"{major}.{minor}.{revision}");
                if (!self.IsPrerelease)
                    return newVersion.Adjust(targetVersion, policy);

                return policy switch
                {
                    VersioningPolicy.Hard => newVersion,
                    VersioningPolicy.Soft => newVersion > targetVersion
                        ? newVersion
                        : targetVersion,
                    _ => throw new ArgumentOutOfRangeException(nameof(policy), policy, null)
                };
            }
        }

        public static NugetPrereleaseVersion? Adjust(
            this NugetPrereleaseVersion self, 
            NugetPrereleaseVersion? other,
            VersioningPolicy policy)
        {
            if (self.IsPattern)
                return adjustWithPattern();

            if (self <= other && policy is VersioningPolicy.Soft)
                return other;

            if (self >= other)
                return self;
            
            return policy switch
            {
                VersioningPolicy.Soft => other,
                VersioningPolicy.Hard => self,
                _ => throw new ArgumentOutOfRangeException(nameof(policy), policy, null)
            };
            
            NugetPrereleaseVersion adjustWithPattern()
            {
                var version = self.Version + (other?.Version ?? 0);
                return new NugetPrereleaseVersion($"{self.Phase.ToString().ToLowerInvariant()}.{version}");
            }
        }
    }
}