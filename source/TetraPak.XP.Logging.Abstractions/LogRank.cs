namespace TetraPak.XP.Logging.Abstractions
{
    /// <summary>
    ///   Used by the (<see cref="ILog"/> based) logging mechanism to classify log entries.
    /// </summary>
    public enum LogRank
    {
        /// <summary>
        ///   The lowest (most detailed) log rank.
        /// </summary>
        Trace,

        /// <summary>
        ///   Logs information to support debugging scenarios.
        /// </summary>
        Debug,

        /// <summary>
        ///   Logs "normal" operations.
        /// </summary>
        Information,

        /// <summary>
        ///   Logs potentially erroneous/invalid/unexpected operations that did not result in a failure.
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
}