using System;

namespace TetraPak.XP.Nuget
{
    public sealed class NugetPrereleaseVersion : MultiStringValue
    {
        public ProjectPhase Phase { get; private set; }

        public int Version { get; set; }

        public bool IsPattern { get; private set; }

        protected override Outcome<string[]> OnValidate(string[] items)
        {
            if (items.Length != 2)
                throw new FormatException("Invalid prerelease format");

            if (!int.TryParse(items[1], out var version))
                throw new FormatException("Invalid prerelease format");

            IsPattern = items[1].StartsWithAny(new[] { '+', '-' });
            if (!items[0].TryParseEnum<ProjectPhase>(out var phase, true))
                throw new FormatException($"Invalid prerelease format. Unknown phase: {items[0]}");
                
            Phase = phase;
            Version = version;
            
            return Outcome<string[]>.Success(items);
        }

        public int Compare(NugetPrereleaseVersion other)
        {
            var comparePhase = Phase.CompareTo(other.Phase); 
            if (comparePhase != 0)
                return comparePhase;

            return Version < other.Version
                ? -1
                : Version > other.Version
                    ? 1
                    : 0;
        }

        public static bool operator <(NugetPrereleaseVersion left, NugetPrereleaseVersion? right) => right is null || left.Compare(right) < 0;

        public static bool operator <=(NugetPrereleaseVersion left, NugetPrereleaseVersion? right) => right is null || left.Compare(right) <= 0;

        public static bool operator >(NugetPrereleaseVersion left, NugetPrereleaseVersion? right) => right is {} && left.Compare(right) > 0;

        public static bool operator >=(NugetPrereleaseVersion left, NugetPrereleaseVersion? right) => right is {} && left.Compare(right) >= 0;

#pragma warning disable CS8618
        public NugetPrereleaseVersion(string stringValue) 
            : base(stringValue, NugetVersion.VersionDefaultSeparator)
        {
        }
#pragma warning restore CS8618
    }
}