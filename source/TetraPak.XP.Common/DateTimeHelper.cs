using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace TetraPak.XP
{
    /// <summary>
    ///   Provides convenient helper methods for working with <see cref="DateTime"/> values.
    /// </summary>
    public static class DateTimeHelper
    {
        /// <summary>
        ///   Used to qualify a standardized string representation of a <see cref="DateTime"/> value
        ///   (see <see cref="ToStandardString"/>). 
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public const string UtcQualifier = "Z";
        
        const string StandardDateTimeFormat1 = "yyyy-MM-ddTHH:mm:ss";
        const string StandardDateTimeFormat2 = "yyyy-MM-dd HH:mm:ss";
        const string StandardDateTimeFormat3 = "yyyy-MM-ddTHH:mm:ss" + UtcQualifier;
        const string StandardDateTimeFormat4 = "yyyy-MM-dd HH:mm:ss" + UtcQualifier;
        // high precision
        const string StandardDateTimeFormat5 = "yyyy-MM-ddTHH:mm:ss.fffffff";
        const string StandardDateTimeFormat6 = "yyyy-MM-dd HH:mm:ss.fffffff";
        const string StandardDateTimeFormat7 = "yyyy-MM-ddTHH:mm:ss.fffffff" + UtcQualifier;
        const string StandardDateTimeFormat8 = "yyyy-MM-dd HH:mm:ss.fffffff" + UtcQualifier;
        static readonly string[] s_standardDateTimeFormats = 
        {
            StandardDateTimeFormat1,
            StandardDateTimeFormat2,
            StandardDateTimeFormat3,
            StandardDateTimeFormat4,
            StandardDateTimeFormat5,
            StandardDateTimeFormat6,
            StandardDateTimeFormat7,
            StandardDateTimeFormat8
        };
            
        static readonly Regex s_timeSpanRegex = new(@"(?<num>[\d\,\.]*)(?<unit>[d,h,m,s,ms]?)", RegexOptions.Compiled);

        
        public static bool TryParseTimeSpan(this string stringValue, string defaultUnit, out TimeSpan timeSpan, CultureInfo? cultureInfo = null)
        {
            timeSpan = TimeSpan.Zero;
            if (string.IsNullOrWhiteSpace(stringValue))
                return false;

            var match = s_timeSpanRegex.Match(stringValue);
            if (!match.Success)
                return false;

            cultureInfo ??= CultureInfo.InvariantCulture;
            var numeric = match.Groups["num"].Value;
            var unit = match.Groups["unit"].Value;
            if (string.IsNullOrEmpty(unit))
            {
                unit = defaultUnit;
            }
                
            if (!double.TryParse(numeric, NumberStyles.Float, cultureInfo, out var dValue))
                return false;

            switch (unit)
            {
                case TimeUnits.Days:
                    timeSpan = TimeSpan.FromDays(dValue);
                    return true;

                case TimeUnits.Hours:
                    timeSpan = TimeSpan.FromHours(dValue);
                    return true;

                case TimeUnits.Minutes:
                    timeSpan = TimeSpan.FromMinutes(dValue);
                    return true;
           
                case TimeUnits.Seconds:
                    timeSpan = TimeSpan.FromSeconds(dValue);
                    return true;

                case TimeUnits.Milliseconds:
                    timeSpan = TimeSpan.FromMilliseconds(dValue);
                    return true;
                
                default: return false;
            }
        }

        public static TimeFrame Subtract(this TimeFrame self, TimeSpan timeSpan) => new TimeFrame(self.From.Subtract(timeSpan), self.To.Subtract(timeSpan));
        
        /// <summary>
        ///   Serializes the <see cref="DateTime"/> value into a 'standard' format that can be easily
        ///   stored and later parsed (<see cref="TryParseStandardDateTime"/>).
        ///   The format is based on the ISO 8601 standard.
        /// </summary>
        /// <param name="value">
        ///   The <see cref="DateTime"/> value to be serialized.
        /// </param>
        /// <param name="options">
        ///   Used to control the resulting format.
        /// </param>
        /// <returns>
        ///   A standardized <see cref="string"/> representation of the <see cref="DateTime"/> value.
        /// </returns>
        /// <seealso cref="TryParseStandardDateTime"/>
        public static string ToStandardString(
            this DateTime value, 
            DateTimeDefaultFormatOptions options = DateTimeDefaultFormatOptions.None)
        {
            var forceUtc = (options & DateTimeDefaultFormatOptions.ForceUtc) == DateTimeDefaultFormatOptions.ForceUtc;
            if (forceUtc && value.Kind != DateTimeKind.Utc)
            {
                value = value.ToUniversalTime();
            }

            var omitTimeQualifier = (options & DateTimeDefaultFormatOptions.OmitTimeQualifier) ==
                                    DateTimeDefaultFormatOptions.OmitTimeQualifier;
            var highPrecision = (options & DateTimeDefaultFormatOptions.HighPrecision) ==
                                DateTimeDefaultFormatOptions.HighPrecision;

            string format;
            if (highPrecision)
            {
                format = omitTimeQualifier
                    ? StandardDateTimeFormat8
                    : StandardDateTimeFormat7;
            }
            else
            {
                format = omitTimeQualifier
                    ? StandardDateTimeFormat2
                    : StandardDateTimeFormat1;
            }
            var stringValue = value.ToString(format);

            return value.Kind == DateTimeKind.Utc
                ? stringValue + UtcQualifier
                : stringValue;
        }

        /// <summary>
        ///   Attempts parsing a standardized <see cref="string"/> representation of a <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="standardDateTimeString">
        ///   The standardized <see cref="string"/> representation of a <see cref="DateTime"/> value.
        /// </param>
        /// <param name="value">
        ///   On success; passes back the resulting <see cref="DateTime"/> value. 
        /// </param>
        /// <returns>
        ///   <c>true</c> if the <paramref name="standardDateTimeString"/> could be successfully parsed;
        ///   otherwise <c>false</c>.
        /// </returns>
        /// <seealso cref="ToStandardString"/>
        public static bool TryParseStandardDateTime(this string standardDateTimeString, out DateTime value)
        {
            return DateTime.TryParseExact(
                standardDateTimeString, 
                s_standardDateTimeFormats, 
                null, 
                DateTimeStyles.None, 
                out value);
        }
    }
}