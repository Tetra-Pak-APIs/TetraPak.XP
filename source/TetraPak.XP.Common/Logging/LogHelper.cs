using System;

namespace TetraPak.XP.Logging
{
    public static class LogHelper
    {
        /// <summary>
        ///   Adds a message of rank <see cref="LogRank.Trace"/>.
        /// </summary>
        /// <param name="log">
        ///   The logger provider.
        /// </param>
        /// <param name="message">
        ///   A textual message to be logged.
        /// </param>
        public static void Trace(this ILog? log, string message) => log?.Write(LogRank.Trace, message);

        /// <summary>
        ///   Adds a message of rank <see cref="LogRank.Debug"/>.
        /// </summary>
        /// <param name="log">
        ///   The logger provider.
        /// </param>
        /// <param name="message">
        ///   A textual message to be logged.
        /// </param>
        public static void Debug(this ILog? log, string message) => log?.Write(LogRank.Debug, message);

        /// <summary>
        ///   Adds a message of rank <see cref="LogRank.Info"/>.
        /// </summary>
        /// <param name="log">
        ///   The logger provider.
        /// </param>
        /// <param name="message">
        ///   A textual message to be logged.
        /// </param>
        public static void Info(this ILog? log, string message) => log?.Write(LogRank.Info, message);

        /// <summary>
        ///   Adds a message of rank <see cref="LogRank.Warning"/>.
        /// </summary>
        /// <param name="log">
        ///   The logger provider.
        /// </param>
        /// <param name="message">
        ///   A textual message to be logged.
        /// </param>
        public static void Warning(this ILog? log, string message) => log?.Write(LogRank.Warning, message);

        /// <summary>
        ///   Adds an <see cref="Exception"/> and message of rank <see cref="LogRank.Error"/>.
        /// </summary>
        /// <param name="log">
        ///   The logger provider.
        /// </param>
        /// <param name="exception">
        ///   An <see cref="Exception"/> to be logged.
        /// </param>
        /// <param name="message">
        ///   A textual message to be logged.
        /// </param>
        public static void Error(this ILog? log, Exception exception, string? message = null)  => log?.Write(LogRank.Error, message!, exception);        
    }
}