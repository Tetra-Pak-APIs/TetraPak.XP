using System;
using TetraPak.XP;
using TetraPak.XP.CLI;
using TetraPak.XP.FileManagement;
using TetraPak.XP.Logging.Abstractions;

namespace nugt.policies
{
    [NugetPolicy(Name)]
    class MoveNugetPolicy : DistributeNugetPolicy // instantiated via NugetPolicy attribute
    {
        const string Name = "move";
        
        protected override bool IsAssumingReleaseBinFolder => true;

        protected override Outcome<Uri> OnResolveRemoteNugetRepository(string uriString)
        {
            // overridden to ensure the source is a local folder
            return Outcome<Uri>.Fail($"Expected local folder (not remote repository: '{uriString}')");
        }
        
        public MoveNugetPolicy(CommandLineArgs args, ILog log) 
            : base(args, log)
        {
            Method = RepositionMethod.Move;
        }
    }
}