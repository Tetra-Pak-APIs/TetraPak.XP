using System;

namespace TetraPak.XP.Configuration
{
    public delegate bool TypedValueParser<T>(string? stringValue, out T value);

    public delegate bool ArbitraryValueParser(string? stringValue, Type targetType, out object? value, object defaultValue);
}