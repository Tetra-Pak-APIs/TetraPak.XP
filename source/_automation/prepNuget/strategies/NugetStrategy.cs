using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using TetraPak.XP;
using TetraPak.XP.Logging;

namespace prepNuget.strategies
{
    abstract class NugetStrategy
    {
        const string ArgPath1 = "-p";
        const string ArgPath2 = "--path";

        const string ArgRecursive1 = "-r";
        const string ArgRecursive2 = "--recursive";
        DirectoryInfo RootFolder { get; set; }
        
        bool IsRecursive { get; set; }

        public ILog Log { get; }

        public abstract Outcome Run();

        public abstract string GetHelp();
        
        public static Outcome<NugetStrategy> Select(string strategyName, string[] args, ILog log)
        {
            if (strategyName.Equals(HarmonizeStrategy.Name, StringComparison.InvariantCultureIgnoreCase))
                return Outcome<NugetStrategy>.Success(new HarmonizeStrategy(args, log));
            
            return Outcome<NugetStrategy>.Fail(new CodedException(Errors.InvalidArgument, $"Unknown strategy: {strategyName}"));
        }

        protected ProjectFile[] GetProjectFiles(Func<ProjectFile,bool>? callback = null)
        {
            if (!RootFolder.Exists)
                return Array.Empty<ProjectFile>();

            var list = new List<ProjectFile>();
            traverseFoldersLookingForNugetProjectFiles(RootFolder, list, callback);
            return list.ToArray();
        }

        void traverseFoldersLookingForNugetProjectFiles(
            DirectoryInfo folder, 
            ICollection<ProjectFile> list,
            Func<ProjectFile,bool>? callback)
        {
            var files = folder.GetFiles("*.csproj");
            foreach (var file in files)
            {
                var projectFile = new ProjectFile(file); // nisse
                if (callback?.Invoke(projectFile) ?? true)
                {
                    list.Add(projectFile);
                }
            }

            if (!IsRecursive)
                return;

            var subFolders = folder.GetDirectories();
            foreach (var subFolder in subFolders)
            {
                traverseFoldersLookingForNugetProjectFiles(subFolder, list, callback);
            }

        }

        protected virtual Outcome TryInit(string[] args)
        {
            if (!args.TryGetValue(out var path, ArgPath1, ArgPath2))
                return Outcome.Fail(new CodedException(Errors.MissingArgument, $"Expected root folder (preceded by {ArgPath1} | {ArgPath2}"));

            var rootFolder = new DirectoryInfo(path);
            if (!rootFolder.Exists)
                return Outcome.Fail(new CodedException(Errors.InvalidArgument, $"Root folder does not exist: {path}"));
            
            RootFolder = rootFolder;
            IsRecursive = args.TryGetFlag(ArgRecursive1, ArgRecursive2);
            return Outcome.Success();
        }
        
        Outcome tryInit(string[] args) => TryInit(args);

        public Outcome Outcome { get; }

#pragma warning disable CS8618
        public NugetStrategy(string[] args, ILog log)
        {
            Outcome = tryInit(args);
            Log = log;
        }
#pragma warning restore CS8618
    }

    class ProjectFile
    {
        XmlDocument _document;

        public FileInfo FileInfo { get; }
        public bool IsBuildingNugetPackage { get; }

        public ProjectFile(FileInfo file)
        {
            FileInfo = file;
            _document = new XmlDocument();
            _document.Load(file.FullName);
            var e = _document.SelectSingleNode("//Propertygroup/GeneratePackageOnBuild");
            IsBuildingNugetPackage = e is {} && e.InnerText.Equals("true", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}