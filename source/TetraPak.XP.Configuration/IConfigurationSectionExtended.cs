using System.Runtime.CompilerServices;

namespace TetraPak.XP.Configuration
{
    public interface IConfigurationSectionExtended : IConfigurationSection
    {
        /// <summary>
        ///   Gets the number of child elements in the configuration section.
        /// </summary>
        int Count { get; }

        TValue? Get<TValue>(TValue? useDefault = default, [CallerMemberName] string? caller = null);
        
        TValue? GetValue<TValue>(string key, TValue? useDefault = default);
    }
}