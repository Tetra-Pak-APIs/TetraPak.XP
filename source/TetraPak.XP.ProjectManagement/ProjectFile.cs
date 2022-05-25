using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using TetraPak.XP.FileManagement;

namespace TetraPak.XP.ProjectManagement
{
    /// <summary>
    ///   Represents a .NET project file, allowing for reading and writing values.
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    public sealed class ProjectFile : XpFileInfo
    {
        /// <summary>
        ///   Specifies the 'PropertyGroup' tag key name.
        /// </summary>
        public const string TagKeyPropertyGroup = "PropertyGroup";
        
        readonly bool _simulateOnly;

        public bool IsBuildingNugetPackage => getBool("GeneratePackageOnBuild");

        /// <summary>
        ///   Gets the name of the project.
        /// </summary>
        public string ProjectName => Name.TrimPostfix(Extension);

        /// <summary>
        ///   Gets the project file as a <see cref="XmlDocument"/>, allowing for custom project file management.
        /// </summary>
        public XmlDocument Document { get; }

        /// <summary>
        ///   Gets or sets the assembly version value.
        /// </summary>
        public Version? AssemblyVersion
        {
            get => getVersion(nameof(AssemblyVersion));
            set => setVersion(nameof(AssemblyVersion), value);
        }

        /// <summary>
        ///   Gets or sets the assembly file version value.
        /// </summary>
        public Version? FileVersion
        {
            get => getVersion(nameof(FileVersion));
            set => setVersion(nameof(FileVersion), value);
        }

        /// <summary>
        ///   Overridden to return the project file name.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Name;

        /// <summary>
        ///   Gets or sets the project version.
        /// </summary>
        public Version? Version
        {
            get => getVersion(nameof(Version));
            set => setVersion(nameof(Version), value);
        }

        Version? getVersion(string key)
        {
            var node = Document.SelectSingleNode($"//{TagKeyPropertyGroup}/{key}");
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
            
            var node = Document.SelectSingleNode($"//{TagKeyPropertyGroup}/{key}");
            if (node is null)
                return;

            node.InnerText = value.ToString();
        }

        bool getBool(string key, bool defaultValue = false)
        {
            var node = Document.SelectSingleNode($"//{TagKeyPropertyGroup}/{key}");
            if (node is null)
                return defaultValue;

            return bool.TryParse(node.InnerText, out var result)
                ? result
                : defaultValue;
        }
        
        /// <summary>
        ///   Saves all changes to the project file.
        /// </summary>
        public Task SaveAsync()
        {
            if (_simulateOnly)
                return Task.CompletedTask;

            using var stream = new FileStream(PhysicalPath, FileMode.Truncate);
            Document.Save(stream);
            return Task.CompletedTask;
        }

        /// <summary>
        ///   Initializes a <see cref="ProjectFile"/> from a specified file.
        /// </summary>
        /// <param name="file">
        ///   The project file info.
        /// </param>
        /// <param name="simulateOnly">
        ///   When set, all changes will be simulated only and will not be persisted in the actual project file.
        ///   This is mainly intended for testing and spikes.
        /// </param>
        public ProjectFile(FileInfo file, bool simulateOnly)
        : base(file)
        {
            _simulateOnly = simulateOnly;
            Document = new XmlDocument();
            Document.Load(PhysicalPath);
        }
    }
}