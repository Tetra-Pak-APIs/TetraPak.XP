
using System;

namespace TetraPak.XP.Logging
{
    /// <summary>
    ///   Used by the (<see cref="ILog"/> based) logging mechanism
    ///   to classify log entries.
    /// </summary>
    public enum LogRank
    {
        /// <summary>
        ///   The lowest (most detailed) log rank.
        /// </summary>
        Trace,
        
        /// <summary>
        ///   Logs information related to debugging.
        /// </summary>
        Debug,

        /// <summary>
        ///   Logs "normal" operations.
        /// </summary>
        Information,

        /// <summary>
        ///   Logs potentially erroneous/invalid operations.
        /// </summary>
        Warning,

        /// <summary>
        ///   Denotes a logged exception.
        /// </summary>
        Error,
        
        /// <summary>
        ///   Denotes any log rank.
        /// </summary>
        Any,
        
        /// <summary>
        ///   Log rank is not specified.
        /// </summary>
        None
    }

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
    
}
