using Microsoft.Extensions.Configuration;
using TetraPak.XP.Configuration;
using TetraPak.XP.Logging.Abstractions;

namespace TetraPak.XP.Logging;

public static class TetraPakLogBaseServiceHelper
{
    /// <summary>
    ///   Resolves a configured <see cref="LogRank"/>.
    /// </summary>
    /// <param name="config">
    ///   (may be null)<br/>
    ///   Used to resolve a configured log level "the Microsoft way" (Logging:LogLevel).
    /// </param>
    /// <param name="useDefault">
    ///   Used when no <see cref="LogRank"/> was configured (or when <paramref name="config"/> is unassigned.
    /// </param>
    /// <returns>
    ///   The resolved <see cref="LogRank"/>.
    /// </returns>
    public static LogRank ResolveDefaultLogRank(this IConfiguration? config, LogRank useDefault)
    {
        if (config is null)
            return useDefault;
                
        var logLevelSection = config.GetSubSection(new ConfigPath(new[] { "Logging", "LogLevel" }));
        if (logLevelSection is null)
            return useDefault;

        var s = logLevelSection.GetNamed<string>("Default");
        if (string.IsNullOrEmpty(s))
            return useDefault;
            
        return s!.TryParseEnum(typeof(LogRank), out var obj) && obj is LogRank logRank
            ? logRank
            : useDefault;
    }
}