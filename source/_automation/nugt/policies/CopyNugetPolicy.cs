using System;
using System.Threading.Tasks;
using TetraPak.XP;
using TetraPak.XP.CLI;
using TetraPak.XP.FileManagement;
using TetraPak.XP.Logging.Abstractions;

namespace nugt.policies
{
    [NugetPolicy(Name)]
    class CopyNugetPolicy : DistributeNugetPolicy // instantiated via NugetPolicy attribute
    {
        const string Name = "copy";

        protected override bool IsAssumingReleaseBinFolder => true;

        
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

        protected override Outcome<Uri> OnResolveRemoteNugetRepository(string uriString)
        {
            // overridden to ensure the source is a local folder
            return Outcome<Uri>.Fail($"Expected local folder (not remote repository: '{uriString}')");
        }

        public CopyNugetPolicy(CommandLineArgs args, ILog log) 
        : base(args, log)
        {
            Method = RepositionMethod.Copy;
        }
    }
}