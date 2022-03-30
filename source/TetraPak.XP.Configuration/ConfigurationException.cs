using System;

namespace TetraPak.XP.Configuration;

/// <summary>
///   Represents errors pertaining to configuration issues.
/// </summary>
public sealed class ConfigurationException : Exception
{
    public ConfigurationException(string message, Exception? inner = null)
    : base(message, inner)
    {
    }
}
