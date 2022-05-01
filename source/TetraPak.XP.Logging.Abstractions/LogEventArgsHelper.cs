namespace TetraPak.XP.Logging.Abstractions
{
    public static class LogEventArgsHelper
    {
        /// <summary>
        ///   (fluent api)<br/>
        ///   Assigns the <see cref="LogEventArgs.Rank"/> property and returns <c>this</c>.
        /// </summary>
        public static LogEventArgs WithRank(this LogEventArgs args, LogRank rank)
        {
            args.Rank = rank;
            return args;
        }
    }
}