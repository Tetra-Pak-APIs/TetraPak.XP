using TetraPak.XP.CLI;
using TetraPak.XP.FileManagement;
using TetraPak.XP.Logging.Abstractions;

namespace nugt.policies
{
    [NugetPolicy(Name)]
    class CopyNugetPolicy : DistributeNugetPolicy // instantiated via NugetPolicy attribute
    {
        const string Name = "copy";
        
        public CopyNugetPolicy(CommandLineArgs args, ILog log) : base(args, log)
        {
            Method = RepositionMethod.Copy;
        }
    }
}