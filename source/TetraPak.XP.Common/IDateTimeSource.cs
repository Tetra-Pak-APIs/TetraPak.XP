using System;

namespace TetraPak.XP
{
    /// <summary>
    ///   Classes implementing this interface can be used to produce date and time values.
    /// </summary>
    public interface IDateTimeSource
    {
        /// <summary>
        ///   Gets a value indicating whether the date/time values returned are skewed (not realtime).  
        /// </summary>
        bool IsTimeSkewed { get; }
        
        /// <summary>
        ///   Gets the current (or skewed) local date and time.
        /// </summary>
        DateTime GetNow();

        /// <summary>
        ///   Gets the current (or skewed) date and time, expressed as the Coordinated Universal Time (UTC).
        /// </summary>
        DateTime GetUtcNow();

        /// <summary>
        ///   Gets the current (or skewed) date that is set to today's date, with the time component set to 00:00:00.
        /// </summary>
        DateTime GetToday();

        /// <summary>
        ///   Attempts pausing the passage of time and returns the outcome.    
        /// </summary>
        /// <returns>
        ///   An <see cref="Outcome"/> to indicate success/failure. A failed outcome also carries an
        ///   <see cref="Outcome.Exception"/> and <see cref="Outcome.Message"/>.
        /// </returns>
        Outcome<DateTime> TryStop();
        
        /// <summary>
        ///   Attempts pausing the passage of time and returns the outcome.    
        /// </summary>
        /// <returns>
        ///   An <see cref="Outcome"/> to indicate success/failure. A failed outcome also carries an
        ///   <see cref="Outcome.Exception"/> and <see cref="Outcome.Message"/>.
        /// </returns>
        Outcome<DateTime> TryResume();
    }
}