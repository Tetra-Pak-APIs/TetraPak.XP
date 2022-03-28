using TetraPak.XP.Logging;

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