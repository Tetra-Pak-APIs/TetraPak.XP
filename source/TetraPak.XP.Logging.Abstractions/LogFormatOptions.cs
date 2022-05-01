namespace TetraPak.XP.Logging.Abstractions
{
    /// <summary>
    ///   Used to specify options controlling formatting of logged events.
    /// </summary>
    public sealed class LogFormatOptions
    {
        /// <summary>
        ///   When set, the event timestamp will not be included in the log event message.
        /// </summary>
        public bool OmitTimestamp { get; set; }

        /// <summary>
        ///   When set, the event source will not be included in the log event message.
        /// </summary>
        public bool OmitSource { get; set; }

        /// <summary>
        ///   When set, the event log rank will not be included in the log event message.
        /// </summary>
        public bool OmitRank { get; set; }

        /// <summary>
        ///   When set, the standard event prefix will not be included in the log event message.
        /// </summary>
        public bool OmitPrefix { get; set; }

        /// <summary>
        ///   When set, the event message id not be included in the log event message.
        /// </summary>
        public bool OmitMessageId { get; set; }

        /// <summary>
        ///   Gets default log format options.
        /// </summary>
        public static LogFormatOptions Default => new();
    }
}