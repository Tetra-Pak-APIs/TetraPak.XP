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
    }
}