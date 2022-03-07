using System;

namespace TetraPak.XP.Configuration
{
    public delegate bool ValueParser(string? stringValue, Type targetType, out object? value, object defaultValue);
}