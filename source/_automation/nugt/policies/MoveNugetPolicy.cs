using TetraPak.XP.CLI;
using TetraPak.XP.FileManagement;
using TetraPak.XP.Logging.Abstractions;

namespace nugt.policies
{
    [NugetPolicy(Name)]
    class MoveNugetPolicy : DistributeNugetPolicy // instantiated via NugetPolicy attribute
    {
        const string Name = "move";
        
        public MoveNugetPolicy(CommandLineArgs args, ILog log) 
        : base(args, log)
        {
            Method = RepositionMethod.Move;
        }
    }
}