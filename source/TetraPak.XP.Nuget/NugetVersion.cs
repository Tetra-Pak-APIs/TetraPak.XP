using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json.Serialization;
using TetraPak.XP.DynamicEntities;
using TetraPak.XP.ProjectManagement;
using TetraPak.XP.Serialization;

namespace TetraPak.XP.Nuget
{
    [JsonConverter(typeof(JsonStringValueSerializer<NugetVersion>))]
    [DebuggerDisplay("{" + nameof(StringValue) + "}")]
    public sealed class NugetVersion : DynamicPath
    {
        /// <summary>
        ///   The default separator used in configuration paths.
        /// </summary>
        public const string VersionDefaultSeparator = ".";
        
        public int Major { get; private set; }

        public int Minor { get; private set; }

        public int Revision { get; private set; }

        public int Build { get; private set; }

        public NugetPrereleaseVersion? Prerelease { get; private set; }

        public bool IsPrerelease => Prerelease is { };

        public new static NugetVersion Empty => new();

        public static implicit operator NugetVersion(string s) => new(s);

        public bool IsPattern { get; private set; }

        public NugetVersion For(NugetPrereleaseVersion? prerelease)
        {
            Prerelease = prerelease;
            return this;
        }
        
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

        bool Equals(NugetVersion other)
        {
            if (!base.Equals(other) && Major == other.Major && Minor == other.Minor && Revision == other.Revision)
                return false;

            return Prerelease?.Equals(other.Prerelease) ?? other.Prerelease is null;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NugetVersion)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ Major;
                hashCode = (hashCode * 397) ^ Minor;
                hashCode = (hashCode * 397) ^ Revision;
                hashCode = (hashCode * 397) ^ Build;
                hashCode = (hashCode * 397) ^ Prerelease?.GetHashCode() ?? 0;
                return hashCode;
            }
        }

        public static bool operator ==(NugetVersion left, NugetVersion right) => Equals(left, right);

        public static bool operator !=(NugetVersion left, NugetVersion right) => !Equals(left, right);

        public static bool operator <(NugetVersion left, NugetVersion right) => left.Compare(right) < 0;

        public static bool operator >(NugetVersion left, NugetVersion right) => left.Compare(right) > 0;

        public static bool operator <=(NugetVersion left, NugetVersion right) => left.Compare(right) <= 0;

        public static bool operator >=(NugetVersion left, NugetVersion right) => left.Compare(right) >= 0;

        protected override Outcome<string[]> OnValidate(string[] items)
        {
            return items.Length switch
            {
                0 => Outcome<string[]>.Success(items),
                3 => compile(items),
                4 => compileForPrerelease(items),
                _ => Outcome<string[]>.Fail(new FormatException("Nuget version must be 3 or four elements"))
            };
        }

        Outcome<string[]> compile(string[] items)
        {
            IsPattern =
                items[0].StartsWithAny(new[] { '+', '-' })
                || items[1].StartsWithAny(new[] { '+', '-' })
                || items[2].StartsWithAny(new[] { '+', '-' })
                || items.Length == 4 && items[3].ContainsAny(new[] { '+', '-' });
                
            if (!int.TryParse(items[0], out var major))
                throw formatError();

            if (!int.TryParse(items[1], out var minor))
                throw formatError();

            if (!int.TryParse(items[2], out var revision))
                throw formatError();

            Major = major;
            Minor = minor;
            Revision = revision;

            return Outcome<string[]>.Success(items);

        }

        static Exception formatError() => new FormatException("Nuget version must be three or four elements");

        Outcome<string[]> compileForPrerelease(string[] items)
        {
            IsPattern =
                items[0].StartsWithAny(new[] { '+', '-' })
                || items[1].StartsWithAny(new[] { '+', '-' })
                || items[2].StartsWithAny(new[] { '+', '-' })
                || items[3].StartsWithAny(new[] { '+', '-' });
            
            var split = items[2].Split(new[] { '-' });
            if (split.Length == 1)
            {
                if (!int.TryParse(items[3], out var build))
                    throw formatError();

                Build = build;
                return compile(items);
            }
            
            items[2] = split[0];
            items[3] = $"{split[1]}.{items[3]}";
            compile(items);
            Prerelease = new NugetPrereleaseVersion(items[3]);
            return Outcome<string[]>.Success(items);
        }

        public static NugetVersion GetFromProjectFile(string filePath)
        {
            var projectFile = new ProjectFile(new FileInfo(filePath), false);
            return projectFile.GetNugetVersion();
        }
        
        NugetVersion() 
        {
        }
        
        public NugetVersion(string stringValue) 
        : base(stringValue, VersionDefaultSeparator)
        {
        }
    }
    
    public static class NugetProjectFileHelper
    {
        public static NugetVersion GetNugetVersion(this ProjectFile projectFile)
        {
            var node = projectFile.Document.SelectSingleNode($"//{ProjectFile.TagKeyPropertyGroup}/PackageVersion");
            return node?.InnerText ?? NugetVersion.Empty;
        }

        public static void SetNugetVersion(this ProjectFile projectFile, NugetVersion value)
        {
            var node = projectFile.Document.SelectSingleNode($"//{ProjectFile.TagKeyPropertyGroup}/PackageVersion");
            if (node is null || node.IsReadOnly)
                return;

            node.InnerText = value.StringValue;
        }
        
    }
}