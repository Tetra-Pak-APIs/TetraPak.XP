    using System;
using Microsoft.Extensions.Configuration;

namespace TetraPak.XP
{
    /// <summary>
    ///   A default implementation of the <see cref="IDateTimeSource"/>. Objects of this class
    ///   can also be used to manipulate date/time such as time acceleration/deceleration and setting
    ///   a custom start time.
    /// </summary>
    public sealed class XpDateTime : IDateTimeSource
    {
        readonly DateTime _realUtcStartTime;
        readonly bool _isCustomStartTime;
        DateTime? _pausedTime;

        public const string ConfigurationSectionKey = nameof(DateTimeSource);
        
        /// <summary>
        ///   Gets the set start date and time. 
        /// </summary>
        public DateTime StartTime { get; }
        
        /// <summary>
        ///   Gets the current time acceleration factor value. 
        /// </summary>
        public double TimeAcceleration { get; }

        /// <inheritdoc />
        public bool IsTimeSkewed => _isCustomStartTime || IsTimeAccelerated;

        bool IsTimeAccelerated => Math.Abs(TimeAcceleration - 1d) > double.Epsilon;
        
        /// <inheritdoc />
        public DateTime GetNow() => getNow(DateTimeKind.Local);

        /// <inheritdoc />
        public DateTime GetUtcNow() => getNow(DateTimeKind.Utc);

        /// <inheritdoc />
        public DateTime GetToday() => getToday();
        
        public Outcome<DateTime> TryStop()
        {
            if (_pausedTime is {})
                return Outcome<DateTime>.Fail("Time was already paused");

            _pausedTime = Now;
            return Outcome<DateTime>.Success(_pausedTime.Value);
        }

        public Outcome<DateTime> TryResume()
        {
            if (_pausedTime is null)
                return Outcome<DateTime>.Fail("Cannot resume time. Time was not stopped");

            var now = _pausedTime.Value;
            _pausedTime = null;
            return Outcome<DateTime>.Success(now);
        }

        /// <summary>
        ///   Gets the current (or skewed) local date and time.
        /// </summary>
        public static DateTime Now => DateTimeSource.Current.GetNow();

        /// <summary>
        ///   Gets a <see cref="DateTime"/> object that is set to the current (or skewed) date and time on
        ///   this computer, expressed as the Coordinated Universal Time (UTC).
        /// </summary>
        public static DateTime UtcNow => DateTimeSource.Current.GetUtcNow();
        
        /// <summary>
        ///   Gets the current (or skewed) date that is set to today's date, with the time component set to 00:00:00.
        /// </summary>
        public static DateTime Today => DateTimeSource.Current.GetToday();


        public static Outcome<DateTime> TryStopTime() => DateTimeSource.Current.TryStop();

        public static Outcome<DateTime> TryResumeTime() => DateTimeSource.Current.TryResume();
        
        DateTime getNow(DateTimeKind kind)
        {
            var now = DateTime.Now;
            if (_pausedTime is { })
                return _pausedTime.Value;
            
            if (!_isCustomStartTime && Math.Abs(TimeAcceleration - 1.0) < double.Epsilon)
                return kind switch
                {
                    DateTimeKind.Unspecified => now,
                    DateTimeKind.Local => now,
                    DateTimeKind.Utc => now.ToUniversalTime(),
                    _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
                };

            var diff = now.ToUniversalTime().Subtract(_realUtcStartTime);
            var elapsed = (long) (diff.Ticks * TimeAcceleration);
            var utcNow = StartTime.Add(TimeSpan.FromTicks(elapsed));
            return kind switch
            {
                DateTimeKind.Unspecified => utcNow.ToLocalTime(),
                DateTimeKind.Local => utcNow.ToLocalTime(),
                DateTimeKind.Utc => utcNow,
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };
        }

        DateTime getToday()
        {
            if (Math.Abs(TimeAcceleration - 1.0) < double.Epsilon)
                return DateTime.Today;

            var now = getNow(DateTimeKind.Local);
            return new DateTime(now.Year, now.Month, now.Day);
        }

        static DateTime? getConfiguredStartTime(IConfiguration configuration, DateTime? useDefault = null)
        {
            var section = configuration.GetSection($"{ConfigurationSectionKey}:{nameof(StartTime)}");
            return string.IsNullOrWhiteSpace(section.Value) || !section.Value.TryParseStandardDateTime(out var value)
                ? useDefault
                : value;
        }

        static double getConfiguredTimeAcceleration(IConfiguration configuration, double useDefault)
        {
            var section = configuration.GetSection($"{ConfigurationSectionKey}:{nameof(TimeAcceleration)}");
            return string.IsNullOrWhiteSpace(section.Value) || !double.TryParse(section.Value, out var value)
                ? useDefault
                : value;
        }

        /// <summary>
        ///   Initializes the <see cref="XpDateTime"/>, optionally with a custom start time and
        ///   time acceleration.
        /// </summary>
        /// <param name="startTime">
        ///   (optional; default = <see cref="DateTime.UtcNow"/>)<br/>
        ///    Specifies a custom start time, allowing for time simulation.
        /// </param>
        /// <param name="timeAcceleration">
        ///   (optional; default=1.0)<br/>
        ///   Sets a custom time acceleration factor. Values more than 1.0 will accelerate system time
        ///   whereas value lower than 1.0 will make system time appear slower than real time.
        /// </param>
        public XpDateTime(DateTime? startTime = null, double timeAcceleration = 1.0)
        {
            _isCustomStartTime = startTime is { };
            _realUtcStartTime = DateTime.UtcNow;
            StartTime = startTime ?? DateTime.UtcNow;
            TimeAcceleration = Math.Max(0d, timeAcceleration);
        }
        
        /// <summary>
        ///   Initializes the <see cref="XpDateTime"/>, configuring its functionality from the
        ///   <see cref="IConfiguration"/> framework.
        /// </summary>
        /// <param name="configuration">
        ///   A configuration framework api.
        /// </param>
        public XpDateTime(IConfiguration configuration)
        : this(getConfiguredStartTime(configuration), getConfiguredTimeAcceleration(configuration, 1d))
        {
        }
    }
}