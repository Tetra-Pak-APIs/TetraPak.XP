using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TetraPak.XP;
using TetraPak.XP.Logging;
using TetraPak.XP.Nuget;

namespace nugt.policies
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    ///   Set all nugets to same version (soft): nugt set-version -p {path} -r -nh -nv 1.0.0-alpha.13 -av nuget
    ///   Set all nugets to same version (hard): nugt set-version -p {path} -r -nh -nv 1.0.0-alpha.13 -av nuget -vp hard
    ///   Set all nugets to next version (hard): nugt set-version -p {path} -r -nh -nv 1.0.0-alpha.+1 -av nuget -vp hard
    ///     This will get the most advanced nuget version, increase it as per pattern, and then ensure all other nugets
    ///     get that same version
    /// </remarks>
    [NugetPolicy(Name)]
    class SetVersionsPolicy : Policy
    {
        const string Name = "set-version";

        const string ArgVersioningPolicy1 = "-vp";
        const string ArgVersioningPolicy2 = "--version-policy";
        // const string ArgHarmonizeNugetVersion1 = "-nh";
        // const string ArgHarmonizeNugetVersion2 = "--nuget-harmonize";
        const string ArgTarget1 = "-t";
        const string ArgTarget2 = "--target";
        const string ArgNugetVersion1 = "-nv";              // <version>|<version pattern>|target
        const string ArgNugetVersion2 = "--nuget-version";  // <version>|<version pattern>|target
        const string ArgAsmVersion1 = "-av";                // <version>|<version pattern>|target|nuget
        const string ArgAsmVersion2 = "--assembly-version"; // <version>|<version pattern>|target|nuget
        const string ParamTarget = "target";
        const string ParamNuget = "nuget";

        string? TargetProjectName { get; set; }

        NugetVersion NugetVersion { get; set; }
        
        VersionPattern? AssemblyVersion { get; set; }

        // bool IsNugetVersionTargeted { get; set; } obsolete

        // bool IsVersionTargeted { get; set; }

        // bool IsNugetVersionHarmonized { get; set; }
        
        bool IsVersionNugetHarmonized { get; set; }
        
        VersioningPolicy VersioningPolicy { get; set; }

        public override async Task<Outcome> RunAsync()
        {
            var projectFiles = getNugetProjectFiles();
            var nugetVersion = NugetVersion;
            
            if (VersioningPolicy == VersioningPolicy.Hard && nugetVersion.IsPattern)
            {
                NugetVersion? hardVersion;
                if (TargetProjectName is { })
                {
                    // all nuget projects gets set to the same version as a specified target project ...
                    var targetProject = projectFiles.FirstOrDefault(pf => pf.ProjectName == TargetProjectName);
                    if (targetProject is null)
                        return Outcome.Fail(new FileNotFoundException($"Target project not found: {TargetProjectName}"));

                    hardVersion = nugetVersion.Adjust(targetProject.NugetVersion, VersioningPolicy);
                }
                else
                {
                    // all nuget projects gets set to the same version as te most advanced project ...
                    hardVersion = getHighestNugetVersionFromProjects(out projectFiles);
                    if (hardVersion is null)
                        return Outcome.Fail("No Nuget projects found");
                }

                nugetVersion = hardVersion;
            }
                
            for (var i = 0; i < projectFiles.Length; i++)
            {
                var pf = projectFiles[i];
                pf.NugetVersion = nugetVersion.Adjust(pf.NugetVersion, VersioningPolicy);
                if (AssemblyVersion is { })
                {
                    pf.Version = AssemblyVersion.Adjust(pf.Version, VersioningPolicy);
                    pf.FileVersion = AssemblyVersion.Adjust(pf.AssemblyVersion, VersioningPolicy);
                    pf.AssemblyVersion = AssemblyVersion.Adjust(pf.AssemblyVersion, VersioningPolicy);
                }
                else if (IsVersionNugetHarmonized)
                {
                    var version = nugetVersion.ToVersion(); 
                    pf.Version = version.Adjust(pf.Version, VersioningPolicy);
                    pf.FileVersion = version.Adjust(pf.FileVersion, VersioningPolicy);
                    pf.AssemblyVersion = version.Adjust(pf.AssemblyVersion, VersioningPolicy);
                }

                await pf.SaveAsync();
                Log.Information($"{pf.Name} nv{pf.NugetVersion} (av:{pf.AssemblyVersion}, fv:{pf.FileVersion}, v:{pf.Version})");
            }
            
            return Outcome.Success();
        }

        NugetVersion? getHighestNugetVersionFromProjects(out ProjectFile[] projectFiles)
        {
            projectFiles = getNugetProjectFiles();
            if (projectFiles.Length == 0)
                return null;
                
            var pf = projectFiles[0];
            var max = pf.NugetVersion;
            for (var i = 1; i < projectFiles.Length; i++)
            {
                pf = projectFiles[i];
                if (max < pf.NugetVersion)
                {
                    max = pf.NugetVersion;
                } 
            }

            return max;
        }

        ProjectFile[] getNugetProjectFiles() => GetProjectFiles(
            projectFile 
            => 
            projectFile.IsBuildingNugetPackage ? FileHelper.GetFilesPolicy.GetAndBreak : FileHelper.GetFilesPolicy.Skip);

        bool tryGetTargetProjectFile([NotNullWhen(true)] out ProjectFile? projectFile)
        {
            var projectFiles = getNugetProjectFiles();
            projectFile = projectFiles.FirstOrDefault(pf =>
            {
                var name = pf.Name.TrimPostfix(".csproj");
                return name.Equals(TargetProjectName, StringComparison.InvariantCultureIgnoreCase);

            });
            return projectFile is { };
        }

        public override string GetHelp()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{ArgTarget1} | {ArgTarget2} <target project name>");
            return sb.ToString();
        }

        protected override Outcome TryInit(string[] args)
        {
            var outcome = base.TryInit(args);
            if (!outcome)
                return outcome;

            // --policy
            if (args.TryGetValue(out var versioningPolicy, ArgVersioningPolicy1, ArgVersioningPolicy2))
            {
                versioningPolicy = string.IsNullOrEmpty(versioningPolicy)
                    ? nameof(VersioningPolicy.Soft)
                    : versioningPolicy;
            
                if (!versioningPolicy.TryParseEnum<VersioningPolicy>(out var policy, true))
                    return Outcome.Fail(new CodedException(
                        Errors.InvalidArgument, $"Invalid value: '{versioningPolicy}'"));
                    
                VersioningPolicy = policy;
            }

            // --target <project name>
            if (args.TryGetValue(out var targetProjectName, ArgTarget1, ArgTarget2))
            {
                TargetProjectName = targetProjectName;
            }

            // IsNugetVersionHarmonized = args.TryGetFlag(ArgHarmonizeNugetVersion1, ArgHarmonizeNugetVersion2); obsolete

            ProjectFile? targetProject = null;
            
            // --nuget <version>|<version pattern>|target
            if (args.TryGetValue(out var nugetVersionString, ArgNugetVersion1, ArgNugetVersion2))
            {
                if (nugetVersionString.Equals(ParamTarget, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (TargetProjectName is null)
                        return Outcome.Fail(new CodedException(
                            Errors.InvalidArgument,
                            $"Expected a target project name (please set {ArgTarget1} or {ArgTarget2})"));
                        
                    if (!tryGetTargetProjectFile(out targetProject))
                        return Outcome.Fail(new CodedException(
                            Errors.InvalidArgument,
                            $"Target project not found: {TargetProjectName}"));
                    
                    NugetVersion = targetProject.NugetVersion;
                }
                else
                {
                    NugetVersion = new NugetVersion(nugetVersionString);
                }
            }
            else
            {
                // no nuget version specified
                return Outcome.Fail(new CodedException(
                    Errors.InvalidArgument,
                    $"Expected nuget version ({ArgNugetVersion1} )"));
            }

            // --assembly-version <version>|<version pattern>|target|nuget 
            if (!args.TryGetValue(out var asmVersionString, ArgAsmVersion1, ArgAsmVersion2)) 
                return checkAtLeastOneVersionIsSpecified();

            // --assembly-version nuget
            if (asmVersionString.Equals(ParamNuget, StringComparison.InvariantCultureIgnoreCase))
            {
                if (NugetVersion.IsEmpty)
                    return Outcome.Fail(new CodedException(
                        Errors.InvalidArgument,
                        $"Expected nuget version ({ArgNugetVersion1})"));

                IsVersionNugetHarmonized = true;
                return Outcome.Success();
            }

            // --assembly-version target
            if (asmVersionString.Equals(ParamTarget, StringComparison.InvariantCultureIgnoreCase))
            {
                if (TargetProjectName is null)
                    return Outcome.Fail(new CodedException(
                        Errors.InvalidArgument,
                        $"Expected a target project name (please set {ArgTarget1} or {ArgTarget2})"));

                if (targetProject is null && !tryGetTargetProjectFile(out targetProject))
                    return Outcome.Fail(new CodedException(
                        Errors.InvalidArgument,
                        $"Target project not found: {TargetProjectName}"));

                AssemblyVersion = targetProject.AssemblyVersion is { }
                    ? new VersionPattern(targetProject.AssemblyVersion)
                    : new VersionPattern(1, 0, 0, 0);

                return Outcome.Success();
            }
            
            // --assembly-version <version>|<version pattern> todo support Version pattern (when required)
            if (!VersionPattern.TryParse(asmVersionString, out var asmVersion))
                return Outcome.Fail(new CodedException(
                    Errors.InvalidArgument, $"Invalid assembly version: {asmVersionString}"));

            AssemblyVersion = asmVersion;
            return Outcome.Success();
        }

        Outcome checkAtLeastOneVersionIsSpecified()
        {
            if (NugetVersion.IsEmpty && AssemblyVersion is null)
                return Outcome.Fail(new CodedException(
                    Errors.MissingArgument, $"No versioning specified. Please state {ArgNugetVersion1} and/or {ArgAsmVersion1}"));

            return Outcome.Success();
        }

        public SetVersionsPolicy(string[] args, ILog log) 
        : base(args, log)
        {
        }
    }
}