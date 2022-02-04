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

        protected override string OnParse(string? stringValue)
        {
            return base.OnParse(stringValue);
        }

        protected override Outcome<string[]> OnValidate(string[] items)
        {
            return items.Length switch
            {
                3 => compile(items),
                4 => compileWithPrerelease(items),
                _ => Outcome<string[]>.Fail(new FormatException("Nuget version must be 3 or four elements"))
            };
        }

        Outcome<string[]> compile(string[] items)
        {
            if (!int.TryParse(items[0], out var major))
                throw formatError();

            if (!int.TryParse(items[0], out var minor))
                throw formatError();

            if (!int.TryParse(items[0], out var revision))
                throw formatError();

            Major = major;
            Minor = minor;
            Revision = revision;

            return Outcome<string[]>.Success(items);

            Exception formatError() => new FormatException("Nuget version must be 3 or four elements");
        }

        Outcome<string[]> compileWithPrerelease(string[] items)
        {
            compile(items);
            Prerelease = new NugetPrerelease(items[4]);
            return Outcome<string[]>.Success(items);
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


        protected override Outcome<string[]> OnValidate(string[] items)
        {
            if (items.Length != 2)
                throw new FormatException("Invalid prerelease format");

            if (!int.TryParse(items[2], out var version))
                throw new FormatException("Invalid prerelease format");
            
            Phase = items[0];
            Version = version;
            
            return Outcome<string[]>.Success(items);
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