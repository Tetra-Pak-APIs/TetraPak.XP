using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using TetraPak.XP;
using TetraPak.XP.Nuget;

namespace nugt.policies
{
    [DebuggerDisplay("{ToString()}")]
    sealed class ProjectFile : XpFileInfo
    {
        const string KeyPropertyGroup = "PropertyGroup";
        
        readonly XmlDocument _document;
        readonly bool _simulateOnly;
        
        public bool IsBuildingNugetPackage { get; }

        public string ProjectName => Name.TrimPostfix(Extension);

        public NugetVersion NugetVersion
        {
            get
            {
                var node =  _document.SelectSingleNode($"//{KeyPropertyGroup}/PackageVersion");
                return node?.InnerText ?? NugetVersion.Empty;
            }
            set
            {
                var node =  _document.SelectSingleNode($"//{KeyPropertyGroup}/PackageVersion");
                if (node is null || node.IsReadOnly)
                    return;

                node.InnerText = value.StringValue;
            }
        }

        public Version? AssemblyVersion
        {
            get => getVersion(nameof(AssemblyVersion));
            set => setVersion(nameof(AssemblyVersion), value);
        }

        public Version? FileVersion
        {
            get => getVersion(nameof(FileVersion));
            set => setVersion(nameof(FileVersion), value);
        }

        public override string ToString() => Name;

        public Version? Version
        {
            get => getVersion(nameof(Version));
            set => setVersion(nameof(Version), value);
        }

        Version? getVersion(string key)
        {
            var node = _document.SelectSingleNode($"//{KeyPropertyGroup}/{key}");
            if (node is null)
                return null;

            return Version.TryParse(node.InnerText, out var version)
                ? version
                : null;
        }
        
        void setVersion(string key, Version? value)
        {
            if (value is null)
                return;
            
            var node = _document.SelectSingleNode($"//{KeyPropertyGroup}/{key}");
            if (node is null)
                return;

            node.InnerText = value.ToString();
        }

        public async Task SaveAsync()
        {
            if (_simulateOnly)
                return;
            
            await using var stream = new FileStream(PhysicalPath, FileMode.Truncate);
            _document.Save(stream);
        }

        public ProjectFile(FileInfo file, bool simulateOnly)
        : base(file)
        {
            _simulateOnly = simulateOnly;
            _document = new XmlDocument();
            _document.Load(PhysicalPath);
            var e = _document.SelectSingleNode($"//{KeyPropertyGroup}/GeneratePackageOnBuild");
            IsBuildingNugetPackage = e is {} && e.InnerText.Equals("true", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}