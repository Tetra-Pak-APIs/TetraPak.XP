using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TetraPak.XP;
using TetraPak.XP.Logging;

namespace nugt.policies
{
    abstract class DistributeNugetPolicy : Policy
    {
        const string ArgNugetVersion1 = "-nv";              // <version>|latest
        const string ArgNugetVersion2 = "--nuget-version";  // <version>|latest
        const string ArgToFolder1 = "-to";                  // <path>
        const string ArgToFolder2 = "--to-folder";          // <path>
        const string ParamLatest = "latest";
        
        protected RepositionMethod Method { get; set; }

        DirectoryInfo? TargetFolder { get; set; }

        public override Task<Outcome> RunAsync()
        {
            foreach (var nugetPackageFile in NugetPackageFiles!)
            {
                switch (Method)
                {
                    case RepositionMethod.Copy:
                        var outcome = nugetPackageFile.CopyTo(TargetFolder!);
                        if (!outcome)
                            return Task.FromResult(outcome);
                        
                        break;
                    
                    case RepositionMethod.Move:
                        outcome = nugetPackageFile.MoveTo(TargetFolder!);
                        if (!outcome)
                            return Task.FromResult(outcome);
                        
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            return Task.FromResult(Outcome.Success());
        }

        public override string GetHelp()
        {
            throw new NotImplementedException();
        }
        
        // NugetPackageFile[] selectLatestNugetPackageFiles() obsolete
        // {
        //     var nugetFilesList = NugetPackageFiles!.ToList();
        //     if (!nugetFilesList.Any())
        //         return Array.Empty<NugetPackageFile>();
        //     
        //     nugetFilesList.Sort((left, right) 
        //         => string.Compare(left.Name, right.Name, StringComparison.Ordinal));
        //
        //     NugetPackageFile? latest = null;
        //     var list = new List<NugetPackageFile>();
        //     foreach (var nugetFile in nugetFilesList)
        //     {
        //         if (latest is {} && nugetFile.Name != latest.Name)
        //         {
        //             list.Add(nugetFile);
        //         }
        //         latest = nugetFile;
        //     }
        //     list.Add(latest!);
        //     return list.ToArray();
        // }
        //
        // NugetPackageFile[] selectNugetPackageFileVersion(NugetVersion version)
        // {
        //     return NugetPackageFiles!.Where(f => f.NugetVersion == version).ToArray();
        // }

        protected override Outcome TryInit(string[] args)
        {
            var baseOutcome = base.TryInit(args);
            if (!baseOutcome)
                return baseOutcome;
            
            if (!args.TryGetValue(out var path, ArgToFolder1, ArgToFolder2))
            {
                TargetFolder = null!;
                return Outcome.Fail(new CodedException(
                    Errors.MissingArgument,
                    $"Expected target folder. Please specify {ArgToFolder1} (or {ArgToFolder2})"));
            }

            TargetFolder = new DirectoryInfo(path);
            if (!TargetFolder.Exists)
                return Outcome.Fail(new DirectoryNotFoundException($"Target folder not found: {TargetFolder}"));

            if (!args.TryGetValue(out var version, ArgNugetVersion1, ArgNugetVersion2))
            {
                version = ParamLatest;
            }
            var packages = new Dictionary<string, NugetPackageFile>();
            GetNugetPackageFiles(f 
                =>
                {
                    if (f.Directory?.Name != "Release")
                        return FileHelper.GetFilesPolicy.Break;

                    if (version != ParamLatest)
                    {
                        if (f.NugetVersion != version) 
                            return FileHelper.GetFilesPolicy.SkipAndContinue;
                        
                        packages[f.NugetName] = f;
                        return FileHelper.GetFilesPolicy.GetAndContinue;
                    }

                    if (!packages.TryGetValue(f.NugetName, out var package))
                    {
                        packages[f.NugetName] = f;
                        return FileHelper.GetFilesPolicy.SkipAndContinue;
                    }

                    if (package.NugetVersion >= f.NugetVersion) 
                        return FileHelper.GetFilesPolicy.SkipAndContinue;
                    
                    packages[f.NugetName] = f;
                    return FileHelper.GetFilesPolicy.SkipAndContinue;
                });


            NugetPackageFiles = packages.Values.ToArray();
            return Outcome.Success();
        }

        public DistributeNugetPolicy(string[] args, ILog log) 
        : base(args, log)
        {
        }
    }
    
    enum RepositionMethod
    {
        Copy,
        
        Move
    }
}