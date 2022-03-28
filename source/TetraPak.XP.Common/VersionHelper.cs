using System;

namespace TetraPak.XP
{
    public static class VersionHelper
    {
        public static Version Adjust(this Version self, Version? targetVersion, VersioningPolicy policy = VersioningPolicy.Soft)
        {
            if (self == targetVersion || targetVersion is null)
                return self;
            
            return policy switch
            {
                VersioningPolicy.Soft => targetVersion < self ? self : targetVersion,
                VersioningPolicy.Hard => self,
                _ => throw new ArgumentOutOfRangeException(nameof(policy), policy, null)
            };
        }
        
        public static Version Adjust(this VersionPattern self, Version? targetVersion, VersioningPolicy policy = VersioningPolicy.Soft)
        {
            var major = self.IsPattern ? self.Major + targetVersion?.Major ?? 0 : self.Major;
            var minor = self.IsPattern ? self.Minor + targetVersion?.Minor ?? 0 : self.Minor;
            var revision = self.IsPattern ? self.Revision + targetVersion?.Revision ?? 0 : self.Revision;
            var build = self.IsPattern ?  self.Build + targetVersion?.Build ?? 0 : self.Build; 
            var newVersion = new Version(major, minor, build, revision);
            return newVersion.Adjust(targetVersion, policy);
        }
    }
}