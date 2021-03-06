using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TetraPak.XP
{
    /// <summary>
    ///   Provides convenient helper methods for working with <see cref="DateTime"/> values.
    /// </summary>
    public static class DateTimeHelper
    {
        static readonly object s_syncRoot = new();
        static bool s_isXpDateTimeAdded;
        
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
            
        static readonly Regex s_timeSpanRegex = new(
            @"(?<num>[\d\,\.]*)(?<unit>d|h|ms|m|s|t)?", 
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        
        /// <summary>
        ///   Attempts parsing the string representation of a <see cref="TimeSpan"/>. 
        /// </summary>
        /// <param name="stringValue">
        ///   The textual <see cref="TimeSpan"/> value.
        /// </param>
        /// <param name="defaultUnit">
        ///   A default time unit if not suffixed in the <paramref name="stringValue"/> (see <see cref="TimeUnits"/> values).
        /// </param>
        /// <param name="value">
        ///   Passes back the parsed <see cref="TimeSpan"/> value on success; otherwise <see cref="TimeSpan.Zero"/>.
        /// </param>
        /// <param name="cultureInfo">
        ///   (optional)<br/>
        ///   A <see cref="CultureInfo"/> value to be used while parsing the <see cref="TimeSpan"/>.
        /// </param>
        /// <param name="ignoreCase">
        ///   (optional; default=<c>false</c>)<br/>
        ///   Specifies whether to ignore character casing while parsing.
        /// </param>
        /// <returns>
        ///   <c>true</c> if <paramref name="stringValue"/> could be successfully parsed into a <see cref="TimeSpan"/>;
        ///   otherwise <c>false</c>.
        /// </returns>
        public static bool TryParseTimeSpan(
            this string stringValue, 
            string defaultUnit, 
            out TimeSpan value,
            CultureInfo? cultureInfo = null,
            bool ignoreCase = false)
        {
            value = TimeSpan.Zero;
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

            if (ignoreCase)
            {
                unit = unit.ToLower();
                if (unit == TimeUnits.Days.ToLower())
                {
                    value = TimeSpan.FromDays(dValue);
                    return true;
                }
            }

            switch (unit)
            {
                case TimeUnits.Days:
                    value = TimeSpan.FromDays(dValue);
                    return true;

                case TimeUnits.Hours:
                    value = TimeSpan.FromHours(dValue);
                    return true;

                case TimeUnits.Minutes:
                    value = TimeSpan.FromMinutes(dValue);
                    return true;

                case TimeUnits.Seconds:
                    value = TimeSpan.FromSeconds(dValue);
                    return true;

                case TimeUnits.Milliseconds:
                    value = TimeSpan.FromMilliseconds(dValue);
                    return true;

                case TimeUnits.Ticks:
                    value = TimeSpan.FromTicks((long)dValue);
                    return true;

                default: return false;
            }
        }

        public static TimeFrame Subtract(this TimeFrame self, TimeSpan timeSpan) => new(self.From.Subtract(timeSpan), self.To.Subtract(timeSpan));
        
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

        /// <summary>
        ///   (fluent api)<br/>
        ///   Adds the <see cref="XpDateTime"/> implementation as the current <see cref="IDateTimeSource"/>
        ///   an returns the service <paramref name="collection"/>.
        /// </summary>
        public static IServiceCollection AddXpDateTime(this IServiceCollection collection)
        {
            lock (s_syncRoot)
            {
                if (s_isXpDateTimeAdded)
                    return collection;

                s_isXpDateTimeAdded = true;
            }

            var provider = collection.BuildServiceProvider();
            var xpDateTime = new XpDateTime(provider.GetService<IConfiguration>());
            DateTimeSource.Current = xpDateTime;    
            collection.AddSingleton<IDateTimeSource>(_ => xpDateTime);

            return collection;
        }

        /// <summary>
        ///   Creates and returns a <see cref="DateTime"/> value that applies the time elements
        ///   (hour, minute and second) from a specified <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="dateTime">
        ///   The value to apply the date elements from (year, month and day).
        /// </param>
        /// <param name="time">
        ///   The value to apply the time elements from (hour, minute and second). 
        /// </param>
        /// <returns>
        ///   A new <see cref="DateTime"/> value.
        /// </returns>
        /// <seealso cref="SetTime(DateTime,int,int,int)"/>
        /// <remarks>
        ///   The returned value also retains the <paramref name="dateTime"/> value's <see cref="DateTime.Kind"/>
        ///   value.
        /// </remarks>
        public static DateTime SetTime(this DateTime dateTime, DateTime time)
            => dateTime.SetTime(time.Hour, time.Minute, time.Second);

        /// <summary>
        ///   Creates and returns a <see cref="DateTime"/> value that applies specified
        ///   time elements (hour, minute and second).
        /// </summary>
        /// <param name="dateTime">
        ///   The value to apply the date elements from (year, month and day).
        /// </param>
        /// <param name="hour">
        ///   The hour element.
        /// </param>
        /// <param name="minute">
        ///   The minute element.
        /// </param>
        /// <param name="second">
        ///   The second element.
        /// </param>
        /// <returns>
        ///   A new <see cref="DateTime"/> value.
        /// </returns>
        /// <remarks>
        ///   The returned value also retains the <paramref name="dateTime"/> value's <see cref="DateTime.Kind"/>
        ///   value.
        /// </remarks>
        /// <seealso cref="SetTime(DateTime,DateTime)"/>
        static DateTime SetTime(this DateTime dateTime, int hour, int minute = 0, int second = 0) =>
            new(dateTime.Year, dateTime.Month, dateTime.Day, hour, minute, second, dateTime.Kind);

        /// <summary>
        ///   Creates and returns a <see cref="DateTime"/> value after zeroing the date elements
        ///   (year. month, day), retaining only its time elements (hour, minute second).
        /// </summary>
        /// <param name="dateTime">
        ///   The <see cref="DateTime"/> to return the time elements from.
        /// </param>
        /// <returns>
        ///   A new <see cref="DateTime"/> value.
        /// </returns>
        /// <remarks>
        ///   The returned value also retains the <paramref name="dateTime"/> value's <see cref="DateTime.Kind"/>
        ///   value.
        /// </remarks>
        public static DateTime GetTime(this DateTime dateTime) => 
            new(1, 1, 1, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Kind);
    }
}