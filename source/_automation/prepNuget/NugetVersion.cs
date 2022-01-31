using System;
using TetraPak.XP;
using TetraPak.XP.DynamicEntities;

namespace prepNuget
{
    public class NugetVersion : DynamicPath
    {
        /// <summary>
        ///   The default separator used in configuration paths.
        /// </summary>
        public const string VersionDefaultSeparator = ".";
        
        public int Major { get; private set; }

        public int Minor { get; private set; }

        public int Revision { get; private set; }

        public NugetPrerelease? Prerelease { get; private set; }

        public bool IsPrerelease => Prerelease is { };

        public static implicit operator NugetVersion(string s) => new(s);

        /// <summary>
        ///   Compares <c>this</c> version with other version and returns a value indicating which is lower/higher
        ///   (see remarks).
        /// </summary>
        /// <param name="other">
        ///   The other <see cref="NugetVersion"/> to compare to <c>this</c>.
        /// </param>
        /// <returns>
        ///   negative value if <c>this</c> version &lt; <paramref name="other"/><br/>
        ///   positive value if <c>this</c> version &gt; <paramref name="other"/><br/>
        ///   zero (0) value if <c>this</c> version is the same as <paramref name="other"/><br/>
        /// </returns>
        public int Compare(NugetVersion other)
        {
            if (Major < other.Major)
                return -1;

            if (Major > other.Major)
                return 1;
            
            if (Minor < other.Minor)
                return -1;

            if (Minor > other.Minor)
                return 1;
            
            if (Revision < other.Revision)
                return -1;

            if (Revision > other.Revision)
                return 1;

            if (!IsPrerelease) 
                return other.IsPrerelease ? 1 : 0;
            
            if (other.IsPrerelease)
                return Prerelease!.Compare(other.Prerelease!);

            return -1;
        }

        protected override string[] OnSetItems(string[]? value)
        {
            if (value is null)
                throw new FormatException("Nuget version cannot be unassigned");

            return value.Length switch
            {
                3 => compile(value),
                4 => compileWithPrerelease(value),
                _ => throw new FormatException("Nuget version must be 3 or four elements")
            };
        }

        string[] compile(string[] value)
        {
            if (!int.TryParse(value[0], out var major))
                throw formatError();

            if (!int.TryParse(value[0], out var minor))
                throw formatError();

            if (!int.TryParse(value[0], out var revision))
                throw formatError();

            Major = major;
            Minor = minor;
            Revision = revision;

            return value;

            Exception formatError() => new FormatException("Nuget version must be 3 or four elements");
        }

        string[] compileWithPrerelease(string[] value)
        {
            compile(value);
            Prerelease = new NugetPrerelease(value[4]);
            return value;
        }

        public NugetVersion(string stringValue) 
        : base(stringValue, VersionDefaultSeparator)
        {
        }
    }

    public class NugetPrerelease : MultiStringValue
    {
        public string Phase { get; private set; }

        public int Version { get; set; }

        protected override string[] OnSetItems(string[]? value)
        {
            if (value is null)
                throw new FormatException("Prerelease must be assigned");
            
            if (value.Length != 2)
                throw new FormatException("Invalid prerelease format");

            if (!int.TryParse(value[2], out var version))
                throw new FormatException("Invalid prerelease format");
            
            Phase = value[0];
            Version = version;
            return value;
        }

        public int Compare(NugetPrerelease other)
        {
            var comparePhase = string.Compare(Phase, other.Phase, StringComparison.InvariantCultureIgnoreCase);
            if (comparePhase != 0)
                return comparePhase;

            return Version < other.Version
                ? 1
                : Version > other.Version
                    ? 1
                    : 0;
        }

#pragma warning disable CS8618
        public NugetPrerelease(string stringValue) 
            : base(stringValue, NugetVersion.VersionDefaultSeparator)
        {
        }
#pragma warning restore CS8618
    }
}