using TetraPak.XP;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Desktop;

[assembly:XpService(typeof(IRuntimeEnvironmentResolver), typeof(DesktopRuntimeEnvironmentResolver))]

namespace TetraPak.XP.Desktop
{
    public sealed class DesktopRuntimeEnvironmentResolver : RuntimeEnvironmentResolver
    {
        // const string TetraPakAppEnvironmentVariable = "DOTNET_ENVIRONMENT"; obsolete
        //
        // public RuntimeEnvironment ResolveRuntimeEnvironment(RuntimeEnvironment useDefault = RuntimeEnvironment.Unknown)
        // {
        //     var s = Environment.GetEnvironmentVariable(TetraPakAppEnvironmentVariable);
        //     if (!s.IsAssigned())
        //         return useDefault;
        //
        //     return Enum.TryParse<RuntimeEnvironment>(s, true, out var value)
        //         ? value
        //         : useDefault;
        // }
    }
}