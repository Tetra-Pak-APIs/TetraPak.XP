using System;

namespace TetraPak.XP.Configuration
{
    public delegate bool TypedValueParser<T>(string? stringValue, out T value);
}