using System;

namespace TetraPak.XP.Logging.Abstractions;

/// <summary>
///   Decorating a property with this attribute indicates the property should not be disclosed
///   in log, traces or similar output when the declaring object's state is presented. 
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class RestrictedValueAttribute : Attribute
{
    /// <summary>
    ///   (default=<see cref="LogRank.None"/>)<br/>
    ///   Specifies the lowest <see cref="LogRank"/> at which the restricted value can be disclosed.
    /// </summary>
    public LogRank DisclosureLogLevel { get; set; } = LogRank.None;
}