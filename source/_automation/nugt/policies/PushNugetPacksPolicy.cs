using System;
using System.Threading.Tasks;
using TetraPak.XP;
using TetraPak.XP.CLI;
using TetraPak.XP.FileManagement;
using TetraPak.XP.Logging.Abstractions;

namespace nugt.policies;

[NugetPolicy(Name)]
sealed class PushNugetPacksPolicy : DistributeNugetPolicy // instantiated via NugetPolicy attribute
{
    const string Name = "copy";
    const string ArgSourcePath1 = "-s";          // <well known domain> (nuget.org) | <uri base path> (eg. https://www.nuget.org/api/v2/)
    const string ArgSourcePath2 = "-source";     // -- " --
    const string ArgApiKey1 = "-ak";             // <key>
    const string ArgApiKey2 = "-api-key";        // -- " --

    public override Task<Outcome> RunAsync()
    {
        foreach (var nugetPackageFile in NugetPackageFiles!)
        {
            return Method switch
            {
                RepositionMethod.Copy => pushAsync(nugetPackageFile, false),
                RepositionMethod.Move => pushAsync(nugetPackageFile, true),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
            
        return Task.FromResult(Outcome.Success());
    }

    Task<Outcome> pushAsync(NugetPackageFile nugetPackageFile, bool removeOnSuccess)
    {
        throw new NotImplementedException();
    }
        
    public PushNugetPacksPolicy(CommandLineArgs args, ILog log) 
        : base(args, log)
    {
    }
}