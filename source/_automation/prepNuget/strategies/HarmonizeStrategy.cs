using System.Text;
using TetraPak.XP;
using TetraPak.XP.Logging;

namespace prepNuget
{
    class HarmonizeStrategy : NugetStrategy
    {
        const string ArgTarget1 = "-t";
        const string ArgTarget2 = "--target";

        string TargetProjectName { get; set; }

        public override Outcome Run()
        {
            throw new System.NotImplementedException();
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

            if (!args.TryGetValue(out var targetProjectName, ArgTarget1, ArgTarget2)) 
                return Outcome.Fail(new CodedException(Errors.MissingArgument, $"Expected target project name (preceded by {ArgTarget1} | {ArgTarget2})"));

            TargetProjectName = targetProjectName;
            return Outcome.Success();
        }

        public HarmonizeStrategy(string[] args, ILog log) 
        : base(args, log)
        {
            ProjectFile? sourceProjectFile = null;
            var projectFiles = GetProjectFiles(projectFile =>
            {
                if (projectFile.FileInfo.Name == TargetProjectName)
                {
                    sourceProjectFile = projectFile;
                }

                return projectFile.IsBuildingNugetPackage;
            });
        }

    }
}