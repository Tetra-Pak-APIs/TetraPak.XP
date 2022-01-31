#nullable enable

namespace TetraPak
{
    /// <summary>
    ///   A string compatible value.
    /// </summary>
    public interface IStringValue
    {
        /// <summary>
        ///   The value's string representation.
        /// </summary>
        string? StringValue { get; } // todo Consider removing the nullability aspect of StringValue (just doesn't make sense)
    }
}