using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using TetraPak.XP;
using TetraPak.XP.FileManagement;
using TetraPak.XP.Logging.Abstractions;
using TetraPak.XP.Nuget;

namespace nugt.policies
{
    [DebuggerDisplay("{ToString()}")]
    public sealed class NugetPackageFile : XpFileInfo
    {
        readonly FileInfo _fileInfo;

        public string NugetName { get; }

        public NugetVersion NugetVersion { get; }

        public override string ToString() => Name;

        (string name, NugetVersion version) parse()
        {
            var name = _fileInfo.Name.TrimPostfix(_fileInfo.Extension);
            var ca = name.ToCharArray();
            
            // skip to first numeric ...
            var sb = new StringBuilder();
            var prev = (char)0;
            for (var i = 0; i < ca.Length; i++)
            {
                if (char.IsNumber(ca[i]) && prev == '.') 
                    return new(sb.ToString().TrimEnd('.'), new NugetVersion(name.Substring(i)));

                prev = ca[i];
                sb.Append(ca[i]);
            }

            throw new FormatException($"Invalid Nuget package file name: \"{PhysicalPath}\"");
        }

        public NugetPackageFile(FileInfo fileInfo, ILog? log = null, bool isSimulation = false)
        : base(fileInfo, log, isSimulation)
        {
            _fileInfo = fileInfo;
            var (name, version) = parse();
            NugetName = name;
            NugetVersion = version;
        }

    }
}