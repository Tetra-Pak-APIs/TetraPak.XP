using System.Runtime.CompilerServices;

namespace TetraPak.XP.Configuration
{
    public interface IConfigurationSection : IConfigurationItem
    {
        /// <summary>
        ///   Gets the number of configuration items supported by the section. 
        /// </summary>
        int Count { get; }
        
        TValue? Get<TValue>(TValue? useDefault = default, [CallerMemberName] string? caller = null);
        
        TValue? GetValue<TValue>(string key, TValue? useDefault = default);

        TValue? GetDerived<TValue>(TValue? useDefault = default, [CallerMemberName] string? caller = null);

        TValue? GetDerivedValue<TValue>(string key, TValue? useDefault = default);
    }

    public interface IConfigurationItem : IConfiguration
    {
        /// <summary>
        /// Gets the key used to identify this this section within its parent section.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Gets the full path to this section within the <see cref="IConfiguration"/> structure.
        /// </summary>
        string Path { get; }
    }
}