namespace TetraPak.XP.Desktop
{
    static class SdkHelper
    {
        const string NugetVersionSource = "$(1.0.0-alpha.15)";

        internal static string NugetPackageVersion => NugetVersionSource.TrimPrefix("$(").TrimPostfix(")");
    }
}