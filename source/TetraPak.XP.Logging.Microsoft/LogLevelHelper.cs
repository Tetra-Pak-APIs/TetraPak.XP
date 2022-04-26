using System;
using Microsoft.Extensions.Logging;
using TetraPak.XP.Logging.Abstractions;

namespace TetraPak.XP.Logging.Microsoft
{
    public static class LogLevelHelper
    {
        public static LogLevel ToLogLevel(this LogRank rank, LogRank useDefault)
        {
            if (useDefault is LogRank.Any or LogRank.None)
                throw new ArgumentException($"Invalid default value: {useDefault}", nameof(useDefault));

            return rank switch
            {
                LogRank.Trace => LogLevel.Trace,
                LogRank.Debug => LogLevel.Debug,
                LogRank.Information => LogLevel.Information,
                LogRank.Warning => LogLevel.Warning,
                LogRank.Error => LogLevel.Error,
                LogRank.Any => useDefault.ToLogLevel(useDefault),
                LogRank.None => useDefault.ToLogLevel(useDefault),
                _ => throw new ArgumentOutOfRangeException(nameof(rank), rank, null)
            };
        }

        public static LogRank ToLogRank(this LogLevel level)
        {
            return level switch
            {
                LogLevel.Trace => LogRank.Trace,
                LogLevel.Debug => LogRank.Debug,
                LogLevel.Information => LogRank.Information,
                LogLevel.Warning => LogRank.Warning,
                LogLevel.Error => LogRank.Error,
                LogLevel.Critical => LogRank.Error,
                LogLevel.None => LogRank.None,
                _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
            };
        }
    }
}