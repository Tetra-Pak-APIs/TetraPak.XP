using System.Collections.Generic;
using System.Threading.Tasks;

namespace TetraPak.XP.Configuration
{
    public interface IConfiguration
    {
        /// <summary>
        ///   Gets a configuration value.
        /// </summary>
        /// <param name="key">
        ///   Identifies the configuration key.
        /// </param>
        /// <param name="useDefault">
        ///   (optional)<br/>
        ///   A default value to be returned if the requested value cannot be resolved.
        /// </param>
        /// <returns>
        ///   The configuration value.
        /// </returns>
        Task<string> GetAsync(string key, string? useDefault = null);

        /// <summary>
        ///   Sets a configuration value.
        /// </summary>
        /// <param name="key">
        ///   Identifies the configuration key.
        /// </param>
        /// <param name="value">
        ///   The value to be applied. 
        /// </param>
        Task SetAsync(string key, string value);

        /// <summary>
        /// Gets a configuration sub-section with the specified key.
        /// </summary>
        /// <param name="key">The key of the configuration section.</param>
        /// <returns>The <see cref="IConfigurationSection"/>.</returns>
        /// <remarks>
        ///     This method will never return <c>null</c>. If no matching sub-section is found with the specified key,
        ///     an empty <see cref="IConfigurationSection"/> will be returned.
        /// </remarks>
        Task<IConfigurationSection> GetSectionAsync(string key);

        /// <summary>
        /// Gets the immediate descendant configuration sub-sections.
        /// </summary>
        /// <returns>The configuration sub-sections.</returns>
        Task<IEnumerable<IConfigurationSection>> GetChildrenAsync();
    }

    public interface IConfigurationSection : IConfiguration
    {
        /// <summary>
        /// Gets the key this section occupies in its parent.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Gets the full path to this section within the <see cref="IConfiguration"/>.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets or sets the section value.
        /// </summary>
        string? Value { get; set; }
    }
}