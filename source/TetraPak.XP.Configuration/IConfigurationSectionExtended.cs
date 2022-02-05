namespace TetraPak.XP.Configuration
{
    public interface IConfigurationSectionExtended : IConfigurationSection
    {
        /// <summary>
        ///   Gets the number of child elements in the configuration section.
        /// </summary>
        int Count { get; }
    }
}