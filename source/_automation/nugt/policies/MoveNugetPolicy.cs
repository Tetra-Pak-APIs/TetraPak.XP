using TetraPak.XP.Logging;
using TetraPak.XP.Logging.Abstractions;

namespace nugt.policies
{
    [NugetPolicy(Name)]
    class MoveNugetPolicy : DistributeNugetPolicy
    {
        const string Name = "move";
        
        public MoveNugetPolicy(string[] args, ILog log) : base(args, log)
        {
            Method = RepositionMethod.Move;
        }
    }
}