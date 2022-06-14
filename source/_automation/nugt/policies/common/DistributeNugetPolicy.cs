using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TetraPak.XP;
using TetraPak.XP.CLI;
using TetraPak.XP.FileManagement;
using TetraPak.XP.Logging.Abstractions;

namespace nugt.policies
{
    abstract class DistributeNugetPolicy : Policy
    {
        const string ArgNugetVersion1 = "-nv";               // <version>|latest
        const string ArgNugetVersion2 = "--nuget-version";   // <version>|latest
        const string ArgSource1 = "-sc";                     // <local folder> | <well known domain> (nuget.org) | <uri base path> (eg. https://www.nuget.org/api/v2/)
        const string ArgSource2 = "-source";                 // -- " --
        const string ArgFilename1 = "-fp";                   // <regex expression> | <simplified pattern (eg. 'hello *')> 
        const string ArgFilename2 = "-filename-pattern";     // -- " --
        // const string ArgToFolder1 = "-to";                // <path> obsolete
        // const string ArgToFolder2 = "--to-folder";        // <path>
        const string ParamLatest = "latest";
        
        protected abstract bool IsAssumingReleaseBinFolder { get; }

        protected RepositionMethod Method { get; set; }

        protected DirectoryInfo? TargetFolder { get; set; }
        
        protected Uri? TargetService { get; set; }

        protected string? FilenamePattern { get; set; }
        
        protected string? SourceValue { get; private set; } 

        public override Task<Outcome> RunAsync() => throw new NotImplementedException();

        public override string GetHelp() => throw new NotImplementedException();

        protected override Outcome TryInit(CommandLineArgs args)
        {
            var baseOutcome = base.TryInit(args);
            if (!baseOutcome)
                return baseOutcome;
            
            if (!args.TryGetValue(out var sourceValue, ArgSource1, ArgSource2))
            {
                TargetFolder = null!;
                return Outcome.Fail(new CodedException(
                    Errors.MissingArgument,
                    $"Expected source (url/name of nuget source/local folder). Please specify {ArgSource1} (or {ArgSource2})"));
            }

            SourceValue = sourceValue;

            if (args.TryGetValue(out var filenamePattern, ArgFilename1, ArgFilename2))
            {
                FilenamePattern = filenamePattern;
            }

            var folderOutcome = OnResolveLocalFolder(sourceValue);
            if (folderOutcome)
            {
                TargetFolder = folderOutcome.Value!;
            }
            else
            {
                var uriOutcome = OnResolveRemoteNugetRepository(sourceValue);
                if (!uriOutcome)
                    return Outcome.Fail($"Unknown nuget source: '{sourceValue}'");

                TargetService = uriOutcome.Value!;
            }

            if (!args.TryGetValue(out var version, ArgNugetVersion1, ArgNugetVersion2))
            {
                version = ParamLatest;
            }
            var packages = new Dictionary<string, NugetPackageFile>();
            var regex = FilenamePattern.IsAssigned()
                ? FilenamePattern.ToRegexFromWildcards(options:RegexOptions.Compiled | RegexOptions.IgnoreCase)
                : null;
            GetNugetPackageFiles(f 
                =>
            {
                if (regex is { } && !regex.IsMatch(f.Name))
                    return FileHelper.GetFilesPolicy.Skip;
                    
                if (IsAssumingReleaseBinFolder && f.Directory?.Name != "Release")
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

        Outcome<DirectoryInfo> isTargetLocalFolder(string value) => OnResolveLocalFolder(value);

        Outcome<Uri> isTargetRemoteNugetRepository(string value) => OnResolveRemoteNugetRepository(value);

        protected abstract Outcome<Uri> OnResolveRemoteNugetRepository(string uriString);

        protected virtual Outcome<DirectoryInfo> OnResolveLocalFolder(string value)
        {
            if (!value.IsAssigned())
                return Outcome<DirectoryInfo>.Fail($"Expected {ArgSource1}/{ArgSource2} parameter");
            
            var folder = new DirectoryInfo(value);
            return folder.Exists
                ? Outcome<DirectoryInfo>.Success(folder)
                : Outcome<DirectoryInfo>.Fail($"Folder does not exist: {value}");
        }

        public DistributeNugetPolicy(CommandLineArgs args, ILog log) 
        : base(args, log)
        {
        }
    }
}