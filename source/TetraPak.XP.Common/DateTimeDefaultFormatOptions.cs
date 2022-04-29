using System;

namespace TetraPak.XP;

/// <summary>
///   Can be used to control the standardized string representation of a <see cref="DateTime"/> value
///   (see <see cref="DateTimeHelper.ToStandardString"/>).
/// </summary>
[Flags]
public enum DateTimeDefaultFormatOptions
{
    /// <summary>
    ///   No options are specified.
    /// </summary>
    None = 0,
            
    /// <summary>
    ///   The <see cref="DateTime"/> value is always converted to UTC before the serialization.
    ///   The result will be qualified as such by a prefix (<see cref="DateTimeHelper.UtcQualifier"/>).
    /// </summary>
    ForceUtc = 1,
            
    /// <summary>
    ///   Specifies that the standardized <see cref="string"/> representation of a <see cref="DateTime"/>
    ///   value should include fractions of a second.
    /// </summary>
    HighPrecision = 2,
            
    /// <summary>
    ///   Specifies that the 'T' qualifier (of the ISO 8601 standard) should be omitted in the
    ///   standardized <see cref="string"/> representation of a <see cref="DateTime"/>. This might
    ///   be useful to improve human readability. 
    /// </summary>
    OmitTimeQualifier = 4
}