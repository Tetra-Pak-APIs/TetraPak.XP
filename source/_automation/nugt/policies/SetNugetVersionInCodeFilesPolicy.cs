using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TetraPak.XP;
using TetraPak.XP.CLI;
using TetraPak.XP.Logging.Abstractions;
using TetraPak.XP.Nuget;

namespace nugt.policies
{
    [NugetPolicy(Name)]
    public sealed class SetNugetVersionInCodeFilesPolicy : Policy // instantiated via NugetPolicy attribute
    {
        const string Name = "to-code";
        const string Pattern = "const string NugetVersionSource = \"$(";
        
        const string ArgBreakOnFirst1 = "-b1";
        const string ArgBreakOnFirst2 = "--break-on-first";

        /// <summary>
        ///   When set, only one .cs file is being adjusted when found to contain <see cref="Pattern"/>. 
        /// </summary>
        bool IsBreakOnFirst { get; set; }
        
        public override async Task<Outcome> RunAsync()
        {
            var projectFiles = GetNugetProjectFiles();
            if (projectFiles.Length is 0)
                return Outcome.Fail($"Root folder \"{RootFolder}\" contains no nuget building projects");

            var projectFile = projectFiles[0];
            var csFiles = RootFolder.GetFiles("*.cs", SearchOption.AllDirectories);
            foreach (var csFile in csFiles)
            {
                try
                {
                    string code;
                    using (var stream = File.OpenText(csFile.FullName))
                    {
                        code = await stream.ReadToEndAsync();
                    }
                    var idxStart = code.IndexOf(Pattern, StringComparison.Ordinal);
                    if (idxStart == -1)
                        continue;

                    var idxEnd = code.IndexOf(')', idxStart + Pattern.Length);
                    if (idxEnd == -1)
                        continue;
                    
                    var sb = new StringBuilder();
                    sb.Append(code[..(idxStart + Pattern.Length)]);
                    sb.Append(projectFile.GetNugetVersion());
                    sb.Append(code[idxEnd..]);
                    if (IsSimulating)
                    {
                        Log.Debug($"Simulates writing code to {csFile.Name}:\n{sb}");
                    }
                    else
                    {
                        var fileMode = File.Exists(csFile.FullName)
                            ? FileMode.Truncate
                            : FileMode.CreateNew;
                        await using var stream = File.Open(csFile.FullName, fileMode);
                        await using var writer = new StreamWriter(stream);
                        await writer.WriteAsync(sb);
                    }
                    if (IsBreakOnFirst)
                        return Outcome.Success();
                }
                catch (Exception ex)
                {
                    return Outcome.Fail(ex);
                }
            }
            return Outcome.Success();
        }

        public override string GetHelp()
        {
            return string.Empty; // todo Returns policy help text
        }

        public SetNugetVersionInCodeFilesPolicy(CommandLineArgs args, ILog log) 
        : base(args, log)
        {
            IsBreakOnFirst = Args.TryGetFlag(ArgBreakOnFirst1, ArgBreakOnFirst2);
        }
    }
}