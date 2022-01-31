using System;
using System.Globalization;

namespace TetraPak
{
    public static class Parsing
    {
        public const char NumberDecimalSeparator = '.';

        static readonly Lazy<IFormatProvider> s_formatProvider = new Lazy<IFormatProvider>(() => new NumberFormatInfo()
        {
            NumberDecimalSeparator = NumberDecimalSeparator.ToString()
        });

        public static IFormatProvider FormatProvider => s_formatProvider.Value;

        public static DateTimeStyles DateTimeStyles { get; } = DateTimeStyles.None;

        public static string ToSerializedString(this DateTime self)
        {
            return self.ToString("s");
        }

        public static string ToSerializedString(this TimeSpan self)
        {
            return self.ToString("c");
        }
    }
}