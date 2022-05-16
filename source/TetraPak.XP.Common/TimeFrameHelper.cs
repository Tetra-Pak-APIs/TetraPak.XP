using System;
using System.Collections.Generic;
using System.Linq;

namespace TetraPak.XP
{
    /// <summary>
    ///   Provides convenient helper methods for working with time frames.
    /// </summary>
    public static class TimeFrameHelper
    {
        /// <summary>
        ///   Compares the extended <see cref="TimeFrame"/> with another time frame and
        ///   returns a value to indicate how they overlap.  
        /// </summary>
        /// <param name="self">
        ///   The extended <see cref="TimeFrame"/>.
        /// </param>
        /// <param name="other">
        ///    <see cref="TimeFrame"/> to be compared with this one.
        /// </param>
        /// <returns>
        ///   A <see cref="Overlap"/> value.
        /// </returns>
        public static Overlap GetOverlap(this TimeFrame self, TimeFrame other)
        {
            if (self.To <= other.From || self.From >= other.To)
                return Overlap.None;

            if (self.From <= other.From && self.To >= other.To)
                return Overlap.Full;

            if (other.From <= self.From && other.To >= self.To)
                return Overlap.Full;

            if (self.From < other.From)
                return Overlap.Start;

            return Overlap.End;
        }

        /// <summary>
        ///   Merges two <see cref="TimeFrame"/> values into one.
        /// </summary>
        /// <param name="self">
        ///   The extended <see cref="TimeFrame"/>.
        /// </param>
        /// <param name="other">
        ///   A <see cref="TimeFrame"/> to be merged with this one.
        /// </param>
        /// <returns>
        ///   A new <see cref="TimeFrame"/> that represents the merged time frames, depending on how
        ///   overlap:
        ///   <list type="table">
        ///     <listheader>
        ///       <term>Overlap</term>
        ///       <description>Returns</description>
        ///     </listheader>
        ///     <item>
        ///       <term><see cref="Overlap.None"/></term>
        ///       <description>The extended time frame (<paramref name="self"/>)</description>
        ///     </item>
        ///     <item>
        ///       <term><see cref="Overlap.Full"/></term>
        ///       <description>
        ///         The extended (<paramref name="self"/>) time frame if it starts before the <paramref name="other"/>
        ///         time frame; otherwise the <paramref name="other"/> time frame.
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term><see cref="Overlap.Start"/> or <see cref="Overlap.End"/></term>
        ///       <description>
        ///         A new <see cref="TimeFrame"/> that starts with the lowest <see cref="TimeFrame.From"/> value
        ///         of the two time frames and ends with the highest <see cref="TimeFrame.To"/> value of the
        ///         two time frames.
        ///       </description>
        ///     </item>
        ///   </list>
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   
        /// </exception>
        public static TimeFrame Merge(this TimeFrame self, TimeFrame other)
        {
            switch (self.GetOverlap(other))
            {
                case Overlap.None:
                    return self;
                
                case Overlap.Full:
                    return self.From <= other.From
                        ? self
                        : other;
                
                case Overlap.Start:
                case Overlap.End:
                    return new TimeFrame(
                        new DateTime(Math.Min(self.From.Ticks, other.From.Ticks)), 
                        new DateTime(Math.Max(self.To.Ticks, other.To.Ticks)));
                    
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static TimeFrame[] GetOverlapping(this TimeFrame self, params TimeFrame[] timeFrames)
            => self.GetOverlapping(true, timeFrames);
        
        public static TimeFrame[] GetOverlapping(this TimeFrame self, bool isSorted, params TimeFrame[] timeFrames)
        {
            if (!isSorted)
            {
                var sorted = timeFrames.ToList();
                sorted.Sort((a, b) => a.CompareTo(b));
                timeFrames = sorted.ToArray();
            }
            var overlapping = new List<TimeFrame>();
            for (var i = 0; i < timeFrames.Length; i++)
            {
                var nextTimeframe = timeFrames[i];
                var overlap = self.GetOverlap(nextTimeframe);
                switch (overlap)
                {
                    case Overlap.None:
                        if (self.To < nextTimeframe.From)
                            continue;

                        return overlapping.ToArray();
                    
                    case Overlap.Full:
                        overlapping.Add(nextTimeframe);
                        if (self.From >= nextTimeframe.From && self.To <= nextTimeframe.To)
                            return overlapping.ToArray();
                        
                        break;
                    
                    case Overlap.Start:
                    case Overlap.End:
                        overlapping.Add(nextTimeframe);
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return overlapping.ToArray();
        }

        /// <summary>
        ///   Subtracts one or more chronologically ordered time frames from this time frame.
        /// </summary>
        /// <param name="self">
        ///   The time frame to be subtracted from.
        /// </param>
        /// <param name="timeFrames">
        ///   The timeframes to be subtracted.
        /// </param>
        /// <returns>
        ///   The difference; zero or more remaining timeframe(s) after <paramref name="timeFrames"/>
        ///   was subtracted from the time frame.
        /// </returns>
        /// <remarks>
        ///   NOTE: The <paramref name="timeFrames"/> are assumed to be chronologically sorted.
        ///   If not then please use <see cref="Subtract(TimeFrame,bool,TimeFrame[])"/> instead.
        /// </remarks>
        public static TimeFrame[] Subtract(this TimeFrame self, params TimeFrame[] timeFrames) 
            => 
                self.Subtract(true, timeFrames);

        /// <summary>
        ///   Subtracts one or more time frames from the time frame while specifying whether
        ///   the timeframe collection is chronologically sorted.
        /// </summary>
        /// <param name="self">
        ///   The time frame to be subtracted from.
        /// </param>
        /// <param name="isSorted">
        ///   Specifies whether <paramref name="timeFrames"/> is chronologically sorted.
        /// </param>
        /// <param name="timeFrames">
        ///   The timeframes to be subtracted.
        /// </param>
        /// <returns>
        ///   The difference; zero or more remaining timeframe(s) after <paramref name="timeFrames"/>
        ///   was subtracted from the time frame.
        /// </returns>
        public static TimeFrame[] Subtract(this TimeFrame self, bool isSorted, params TimeFrame[] timeFrames)
        {
            var list = new List<TimeFrame>();
            if (!timeFrames.Any())
                return new[] { self };
            
            if (!isSorted)
            {
                var sorted = timeFrames.ToList();
                sorted.Sort((a, b) => a.CompareTo(b));
                timeFrames = sorted.ToArray();
            }
            for (var i = 0; i < timeFrames.Length; i++)
            {
                var nextTimeFrame = timeFrames[i];
                var diff = self.Subtract(nextTimeFrame);
                switch (diff.Length)
                {
                    case 0:
                        // time frame is either not overlapping or is fully overlapped by the timeframe 
                        return Array.Empty<TimeFrame>();
                    
                    case 1:
                        if (diff[0].From >= nextTimeFrame.To)
                        {
                            self = diff[0];
                            continue;
                        }
                        list.AddRange(diff);
                        return list.ToArray();

                    case 2:
                        if (i == timeFrames.Length - 1)
                        {
                            list.AddRange(diff);
                            return list.ToArray();
                        }
                        list.Add(diff[0]);
#if NET5_0_OR_GREATER                        
                        self = diff[^1];
#else
                        self = diff[diff.Length-1];
#endif                        
                        break;
                }
            }

            return list.ToArray();
        }

        public static TimeFrame[] Subtract(this TimeFrame self, TimeFrame other)
        {
            switch (self.GetOverlap(other))
            {
                case Overlap.None:
                    return new[] { self };
                
                case Overlap.Full:
                    var subtracted = new List<TimeFrame>();
                    if (self.From < other.From)
                    {
                        subtracted.Add(new TimeFrame(self.From, other.From));
                    }
                    if (self.To > other.To)
                    {
                        subtracted.Add(new TimeFrame(other.To, self.To));
                    }
                    return subtracted.ToArray();

                case Overlap.Start:
                    return self.From < other.From 
                        ? new[] { new TimeFrame(self.From, other.From) } 
                        : Array.Empty<TimeFrame>();

                case Overlap.End:
                    return self.To > other.To
                        ? new[] { new TimeFrame(other.To, self.To) } 
                        : Array.Empty<TimeFrame>();

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}