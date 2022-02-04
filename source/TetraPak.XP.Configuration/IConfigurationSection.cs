namespace TetraPak.XP.Configuration
{
    public interface IConfigurationSection : IConfiguration
    {
        /// <summary>
        /// Gets the key used to identify this this section within its parent section.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Gets the full path to this section within the <see cref="IConfiguration"/> structure.
        /// </summary>
        ConfigPath Path { get; }

        /// <summary>
        /// Gets or sets the section value.
        /// </summary>
        string? Value { get; set; }
    }
}