namespace TetraPak.XP.Serialization
{
    /// <summary>
    ///   Dynamic (see <see cref="DynamicEntity"/>) entities implementing this interface allows for
    ///   automatic renaming of properties during serialization by the <see cref="DynamicEntityJsonConverter{T}"/>. 
    /// </summary>
    /// <remarks>
    ///   Implementing this interface is a cheap and efficient means to allow your app service to force a
    ///   naming convention (such as camel case, Pascal case, snake case etc.) without having to declare
    ///   additional classes for output.
    /// </remarks>
    public interface ISerializationKeyMapProvider
    {
        /// <summary>
        ///   Returns a <see cref="KeyMapInfo"/> object with a key map and other mapping settings.
        /// </summary>
        KeyMapInfo GetKeyMap();
    }
}