using TetraPak.XP.Logging;
using TetraPak.XP.Logging.Abstractions;

namespace nugt.policies
{
    [NugetPolicy(Name)]
    class CopyNugetPolicy : DistributeNugetPolicy
    {
        const string Name = "copy";
        
        public CopyNugetPolicy(string[] args, ILog log) : base(args, log)
        {
            Method = RepositionMethod.Copy;
        }
    }
}