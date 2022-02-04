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
        Task<TValue?> GetAsync<TValue>(string key, TValue? useDefault = default);

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
        /// Gets the immediate descendant configuration sub-sections (serialized as JSON objects).
        /// </summary>
        /// <returns>
        ///   The configuration sub-sections.
        /// </returns>
        /// <remarks>
        ///   Please note that this method will not return any scalar values found in the textual
        ///   (JSON) representation of your configuration. Consider this example:
        /// <code>
        ///   {
        ///     "StringValue": "Hello World!",
        ///     "StringArray": [
        ///       "String 1"
        ///       "String 2"
        ///     ],
        ///     "ObjectArray": [
        ///       {
        ///         "Name": "Object 1",
        ///         "Id": 1234 
        ///       },
        ///       {
        ///         "Name": "Object 2",
        ///         "Id": 1235 
        ///       }
        ///     ],
        ///     "Logging": {
        ///       "LogLevel": {
        ///         "Default": "Debug",
        ///         "System": "Information",
        ///         "Microsoft": "Information"
        ///       }
        ///     }
        ///   }
        /// </code>
        ///   This example is perfectly valid JSON and the resulting configuration item
        ///   will support all those values but only one - "Logging" - is considered a
        ///   "child configuration section". So, invoking the <see cref="GetChildrenAsync"/>
        ///   method should only return one item (which, in turn, contains one child configuration:
        ///   "LogLevel").
        /// </remarks>
        Task<IEnumerable<IConfigurationSection>> GetChildrenAsync();
    }

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

    public interface IExtendedConfigurationSection
    {
        /// <summary>
        ///   Gets the number of child elements in the configuration section.
        /// </summary>
        int Count { get; }
    }
}