    using System;
using Microsoft.Extensions.Configuration;

namespace TetraPak.XP
{
    public sealed class XpDateTime : IDateTimeSource
    {
        readonly DateTime _startTime;
        readonly DateTime _realStartTime;

        public static IDateTimeSource Current { get; set; } = new XpDateTime();

        public DateTime StartTime { get; }
        
        public double TimeAcceleration { get; }
        
        public DateTime GetNow() => getNow(DateTimeKind.Local);

        public DateTime GetUtcNow() => getNow(DateTimeKind.Utc);

        public DateTime GetToday() => getToday();

        public static DateTime Now => Current.GetNow();

        public static DateTime UtcNow => Current.GetUtcNow();

        public static DateTime Today => Current.GetToday();
        
        DateTime getNow(DateTimeKind kind)
        {
            if (Math.Abs(TimeAcceleration - 1.0) < double.Epsilon)
                return kind switch
                {
                    DateTimeKind.Unspecified => DateTime.Now,
                    DateTimeKind.Local => DateTime.Now,
                    DateTimeKind.Utc => DateTime.UtcNow,
                    _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
                };
            
            var now = XpDateTime.Now;
            if (Math.Abs(TimeAcceleration - 1.0d) < double.Epsilon)
                return now;

            var diff = now.Subtract(_startTime);
            var elapsed = (long) (diff.Ticks * TimeAcceleration);
            return _startTime.Add(TimeSpan.FromTicks(elapsed));
        }

        DateTime getToday()
        {
            if (Math.Abs(TimeAcceleration - 1.0) < double.Epsilon)
                return DateTime.Today;

            var now = getNow(DateTimeKind.Local);
            return new DateTime(now.Year, now.Month, now.Day);
        }

        static DateTime getConfiguredStartTime(IConfiguration configuration, DateTime useDefault)
        {
            var section = configuration.GetSection($"{nameof(XpDateTime)}:{nameof(StartTime)}");
            return string.IsNullOrWhiteSpace(section.Value) || !section.Value.TryParseStandardDateTime(out var value)
                ? useDefault
                : value;
        }

        static double getConfiguredTimeAcceleration(IConfiguration configuration, double useDefault)
        {
            var section = configuration.GetSection($"{nameof(XpDateTime)}:{nameof(TimeAcceleration)}");
            return string.IsNullOrWhiteSpace(section.Value) || !double.TryParse(section.Value, out var value)
                ? useDefault
                : value;
        }

        public XpDateTime(DateTime? startTime = null, double acceleration = 1.0)
        {
            _startTime = startTime ?? DateTime.UtcNow;
            _realStartTime = DateTime.UtcNow;
            TimeAcceleration = Math.Max(0d, acceleration);
        }
        
        public XpDateTime(IConfiguration configuration)
        : this(getConfiguredStartTime(configuration, DateTime.UtcNow), getConfiguredTimeAcceleration(configuration, 1d))
        {
        }
    }
}