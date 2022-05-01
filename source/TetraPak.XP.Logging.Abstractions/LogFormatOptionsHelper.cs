namespace TetraPak.XP.Logging.Abstractions
{
    /// <summary>
    ///   Provides convenient helper methods for working with <see cref="LogFormatOptions"/>.
    /// </summary>
    public static class LogFormatOptionsHelper
    {
        /// <summary>
        ///   (fluent api)<zbr/>
        ///   Sets/clears the <see cref="LogFormatOptions.OmitTimestamp"/> value
        ///   and returns the <see cref="LogFormatOptions"/>.
        /// </summary>
        public static LogFormatOptions WithOmitTimestamp(this LogFormatOptions options, bool value)
        {
            options.OmitTimestamp = value;
            return options;
        }
        
        /// <summary>
        ///   (fluent api)<zbr/>
        ///   Sets/clears the <see cref="LogFormatOptions.OmitSource"/> value
        ///   and returns the <see cref="LogFormatOptions"/>.
        /// </summary>
        public static LogFormatOptions WithOmitSource(this LogFormatOptions options, bool value)
        {
            options.OmitSource = value;
            return options;
        }
        
        /// <summary>
        ///   (fluent api)<zbr/>
        ///   Sets/clears the <see cref="LogFormatOptions.OmitRank"/> value
        ///   and returns the <see cref="LogFormatOptions"/>.
        /// </summary>
        public static LogFormatOptions WithOmitRank(this LogFormatOptions options, bool value)
        {
            options.OmitRank = value;
            return options;
        }
        
        /// <summary>
        ///   (fluent api)<zbr/>
        ///   Set/clears the <see cref="LogFormatOptions.OmitPrefix"/> value
        ///   and returns the <see cref="LogFormatOptions"/>.
        /// </summary>
        public static LogFormatOptions WithOmitPrefix(this LogFormatOptions options, bool value)
        {
            options.OmitPrefix = value;
            return options;
        }
        
        /// <summary>
        ///   (fluent api)<zbr/>
        ///   Set/clears the <see cref="LogFormatOptions.OmitMessageId"/> value
        ///   and returns the <see cref="LogFormatOptions"/>.
        /// </summary>
        public static LogFormatOptions WithOmitMessageId(this LogFormatOptions options, bool value)
        {
            options.OmitMessageId = value;
            return options;
        }
    }
}