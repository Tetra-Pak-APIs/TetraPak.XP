// using System;
//
// namespace TetraPak.XP
// {
//     public sealed class VersionPattern : StringValueBase
//     {
//         const char Separator = '.';
//
//         public int Major { get; private set; }
//         
//         public int Minor { get; private set; }
//         
//         public int Revision { get; private set; }
//
//         public int Build { get; private set; }
//
//         public bool IsPattern { get; set; }
//
//         protected override StringValueParseResult OnParse(string? stringValue)
//         {
//             if (string.IsNullOrWhiteSpace(stringValue))
//                 throw new ArgumentNullException();
//
//             if (!tryParse(stringValue, out var major, out var minor, out var revision, out var build))
//                 throw new FormatException($"Invalid {GetType()}: \"{stringValue}\"");
//
//             Major = major;
//             Minor = minor;
//             Revision = revision;
//             Build = build;
//             return base.OnParse(stringValue);
//         }
//
//         public static bool TryParse(string? stringValue, out VersionPattern? value)
//         {
//             if (!tryParse(stringValue, out var major, out var minor, out var revision, out var build))
//             {
//                 value = null;
//                 return false;
//             }
//                 
//             value = new VersionPattern($"{major}{Separator}{minor}{Separator}{revision}{Separator}{build}");
//             return true;
//         }
//
//         static bool tryParse(string? stringValue, out int major, out int minor, out int revision, out int build)
//         {
//             major = minor = revision = build = 0;
//             if (string.IsNullOrWhiteSpace(stringValue))
//                 return false;
//
//             var split = stringValue!.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries);
//             if (split.Length != 4)
//                 return false;
//
//             if (!int.TryParse(split[0], out major))
//                 return false;
//             
//             if (!int.TryParse(split[1], out minor))
//                 return false;
//             
//             if (!int.TryParse(split[2], out revision))
//                 return false;
//             
//             if (!int.TryParse(split[3], out build))
//                 return false;
//
//             return true;
//         }
//
//         public VersionPattern(string? stringValue) 
//         : base(stringValue)
//         {
//         }
//
//         public VersionPattern(int major, int minor, int revision, int build) 
//         : this($"{major}{Separator}{minor}{Separator}{revision}{Separator}{build}")
//         {
//         }
//
//         public VersionPattern(Version version) 
//         : this(version.Major, version.Minor, version.Revision, version.Build)
//         {   
//         }
//     }
// }