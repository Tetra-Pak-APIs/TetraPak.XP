namespace TetraPak.XP.Web.Diagnostics
{
    /// <summary>
    ///   Classes implementing this interface can be used to provide service diagnostics.
    /// </summary>
    public interface ITetraPakDiagnosticsProvider
    {
        /// <summary>
        ///   Starts a specified timer.
        /// </summary>
        /// <param name="timerKey">
        ///   Identifies the timer to be started.
        /// </param>
        void DiagnosticsStartTimer(string timerKey);

        /// <summary>
        ///   Stops a specified timer.
        /// </summary>
        /// <param name="timerKey">
        ///   Identifies the timer to be stopped.
        /// </param>
        long? DiagnosticsStopTimer(string timerKey);
    }
}