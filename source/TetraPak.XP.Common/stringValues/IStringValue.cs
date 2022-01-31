#nullable enable

namespace TetraPak.XP
{
    /// <summary>
    ///   A string compatible value.
    /// </summary>
    public interface IStringValue
    {
        /// <summary>
        ///   The value's string representation.
        /// </summary>
        string StringValue { get; }
    }
}