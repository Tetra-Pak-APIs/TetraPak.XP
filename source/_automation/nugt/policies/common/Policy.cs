using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TetraPak.XP;
using TetraPak.XP.Logging;
using TetraPak.XP.Logging.Abstractions;

namespace nugt.policies
{
    abstract class Policy
    {
        const string ArgPath1 = "-p";
        const string ArgPath2 = "--path";

        const string ArgRecursive1 = "-r";
        const string ArgRecursive2 = "--recursive";

        const string ArgSimulate1 = "-sim";
        const string ArgSimulate2 = "--simulate";

        protected ProjectFile[]? ProjectFiles { get; private set; }
        protected NugetPackageFile[]? NugetPackageFiles { get;  set; }

        DirectoryInfo RootFolder { get; set; }
        
        bool IsRecursive { get; set; }
        
        protected bool IsSimulating { get; private set; }

        public string[] Args { get; }

        public ILog Log { get; }
        

        public abstract Task<Outcome> RunAsync();

        public abstract string GetHelp();
        
        protected ProjectFile[] GetProjectFiles(Func<ProjectFile,FileHelper.GetFilesPolicy>? callback = null)
        {
            if (ProjectFiles is { })
                return ProjectFiles;
                
            if (!RootFolder.Exists)
                return Array.Empty<ProjectFile>();

            return ProjectFiles = getProjectFilesRecursively(callback);
        }

        protected NugetPackageFile[] GetNugetPackageFiles(Func<NugetPackageFile,FileHelper.GetFilesPolicy>? callback = null)
        {
            if (NugetPackageFiles is { })
                return NugetPackageFiles;
                
            if (!RootFolder.Exists)
                return Array.Empty<NugetPackageFile>();

            NugetPackageFiles = getNugetFilesRecursively(callback);
            return NugetPackageFiles;
        }

        ProjectFile[] getProjectFilesRecursively(Func<ProjectFile,FileHelper.GetFilesPolicy>? callback)
        {
            var list = new List<ProjectFile>();
            RootFolder.GetAllFiles(
                "*.csproj", 
                info =>
                {
                    if (callback is null)
                    {
                        list.Add(new ProjectFile(info, IsSimulating));
                        return FileHelper.GetFilesPolicy.GetAndBreak;
                    }

                    var pf = new ProjectFile(info, IsSimulating);
                    var result = callback(pf);
                    if ((result & FileHelper.GetFilesPolicy.Get) == FileHelper.GetFilesPolicy.Get)
                    {
                        list.Add(pf);
                    }
                    return result;
                });
            return list.ToArray();
        }

        NugetPackageFile[] getNugetFilesRecursively(Func<NugetPackageFile, FileHelper.GetFilesPolicy>? callback)
        {
            var list = new List<NugetPackageFile>();
            RootFolder.GetAllFiles(
                "*.nupkg", 
                info =>
                {
                    if (callback is null)
                    {
                        list.Add(new NugetPackageFile(info, Log, IsSimulating));
                        return FileHelper.GetFilesPolicy.GetAndContinue;
                    }

                    var npf = new NugetPackageFile(info, Log, IsSimulating);
                    var result = callback(npf);
                    if ((result & FileHelper.GetFilesPolicy.Get) == FileHelper.GetFilesPolicy.Get)
                    {
                        list.Add(npf);
                    }
                    return result;
                });
            return list.ToArray();
        }

        // static FileInfo[] getFilesRecursively(
        //     DirectoryInfo folder, 
        //     // ICollection<FileInfo> list,
        //     string fileSuffix,
        //     Func<FileInfo,FileHelper.GetFilesPolicy>? callback)
        // {
        //     return folder.GetAllFiles(fileSuffix, info => callback?.Invoke(info) ?? FileHelper.GetFilesPolicy.Get);
        // }

        protected virtual Outcome TryInit(string[] args)
        {
            if (!args.TryGetValue(out var path, ArgPath1, ArgPath2))
                return Outcome.Fail(new CodedException(Errors.MissingArgument, $"Expected root folder (preceded by {ArgPath1} | {ArgPath2}"));

            IsSimulating = args.TryGetFlag(ArgSimulate1, ArgSimulate2);
            
            var rootFolder = new DirectoryInfo(path);
            if (!rootFolder.Exists)
                return Outcome.Fail(new CodedException(Errors.InvalidArgument, $"Root folder does not exist: {path}"));
            
            RootFolder = rootFolder;
            IsRecursive = args.TryGetFlag(ArgRecursive1, ArgRecursive2);
            return Outcome.Success();
        }
        
        Outcome tryInit(string[] args) => TryInit(args);

        public Outcome OutcomeFromInit { get; protected set; }

#pragma warning disable CS8618
        public Policy(string[] args, ILog log)
        {
            Args = args; 
            Log = log;
            OutcomeFromInit = tryInit(args);
        }

#pragma warning restore CS8618
    }
}