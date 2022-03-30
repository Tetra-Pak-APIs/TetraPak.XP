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
    }
}