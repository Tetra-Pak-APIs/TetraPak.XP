using System;

namespace TetraPak.XP.Logging.Abstractions;

/// <summary>
///   Provides convenient helper methods for working with <see cref="LogRank"/>. 
/// </summary>
public static class LogRankHelper
{
    /// <summary>
    ///   Returns an abbreviated string representation for the <see cref="LogRank"/> 
    /// </summary>
    /// <exception cref="NotSupportedException">
    ///   The <see cref="LogRank"/> value is not (yet) supported.
    /// </exception>
    public static string ToAbbreviatedString(this LogRank rank)
    {
        return rank switch
        {
            LogRank.Trace => "TRC",
            LogRank.Debug => "DBG",
            LogRank.Information => "INF",
            LogRank.Warning => "WRN",
            LogRank.Error => "ERR",
            LogRank.Any => "---",
            _ => throw new NotSupportedException($"Unsupported log rank: {rank}")
        };
    }
}