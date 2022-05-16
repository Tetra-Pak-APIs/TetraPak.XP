using System;
using System.Diagnostics;

namespace TetraPak.XP
{
    /// <summary>
    ///   Represents a period of time, from start date/time to an end date/time.
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    public sealed class TimeFrame : IComparable<TimeFrame>
    {
        /// <summary>
        ///   The start of the time frame.
        /// </summary>
        public DateTime From { get; }
        
        /// <summary>
        ///   The end of the time frame.
        /// </summary>
        public DateTime To { get; }

        /// <summary>
        ///   Compares this time frame to another time frame and returns a value to indicate they are semantically equal.
        /// </summary>
        /// <param name="other">
        ///   A time frame to be compared with this one.
        /// </param>
        /// <returns>
        ///   <c>true</c> if <paramref name="other"/> is either referencing this time frame
        ///   or if it is semantically equal to this one;
        ///   otherwise <c>false</c>.
        /// </returns>
        bool Equals(TimeFrame other) => From.Equals(other.From) && To.Equals(other.To);

        /// <summary>
        ///   Compares this time frame to another time frame and returns an integer that indicates
        ///   their relative position in a sort order.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>
        ///   A 32-bit signed integer indicating the relationship between the two time frames.
        ///   <list type="table">
        ///     <listheader>
        ///       <term>Value</term>
        ///       <description>Condition</description>
        ///     </listheader>
        ///     <item>
        ///       <term>Less than zero</term>
        ///       <description>This time frame starts before the other time frame.</description>
        ///     </item>
        ///     <item>
        ///       <term>Zero</term>
        ///       <description>This time frame starts at the same date/time as the other time frame.</description>
        ///     </item>
        ///     <item>
        ///       <term>Greater than zero</term>
        ///       <description>This time frame starts after the other time frame.</description>
        ///     </item>
        ///   </list>
        /// </returns>
        public int CompareTo(TimeFrame? other)
        {
            if (ReferenceEquals(this, other)) return 0;
            return ReferenceEquals(null, other) ? 1 : From.CompareTo(other.From);
        }

        /// <summary>
        ///   Compares this time frame to another object and returns a value to indicate they are semantically equal.
        /// </summary>
        /// <param name="obj">
        ///   An arbitrary object.
        /// </param>
        /// <returns>
        ///   <c>true</c> if the arbitrary object is either referencing this time frame or the arbitrary
        ///   object is also a <see cref="TimeFrame"/> that is semantically equal to this one;
        ///   otherwise <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((TimeFrame)obj);
        }

        /// <inheritdoc />
#if NET5_0_OR_GREATER        
        public override int GetHashCode() => HashCode.Combine(From, To);
#else
        public override int GetHashCode()
        {
            unchecked
            {
                return (From.GetHashCode() * 397) ^ To.GetHashCode();
            }
        }
#endif        

        public static bool operator ==(TimeFrame? left, TimeFrame? right) => Equals(left, right);

        public static bool operator !=(TimeFrame? left, TimeFrame? right) => !Equals(left, right);

        /// <inheritdoc />
        public override string ToString() => $"{From:s} -- {To:s}";

        /// <summary>
        ///   Initialises the <see cref="TimeFrame"/> value.
        /// </summary>
        /// <param name="from">
        ///   Initialises the <see cref="From"/> property.
        /// </param>
        /// <param name="to">
        ///   Initialises the <see cref="To"/> property.
        /// </param>
        public TimeFrame(DateTime from, DateTime to)
        {
            From = from;
            To = to;
        }
    }
}