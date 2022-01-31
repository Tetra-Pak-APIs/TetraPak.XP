using System.Diagnostics;
using TetraPak.XP.DynamicEntities;

namespace TetraPak.XP.Configuration
{
    /// <summary>
    ///   String value, representing a configuration path.
    /// </summary>
    [DebuggerDisplay("{" + nameof(StringValue) + "}")]
    public class ConfigPath : DynamicPath 
    {
        /// <summary>
        ///   The default separator used in configuration paths.
        /// </summary>
        public const string ConfigDefaultSeparator = ":";
        
        /// <summary>
        ///   Implicit type cast <see cref="string"/> => <see cref="ConfigPath"/>.
        /// </summary>
        public static implicit operator ConfigPath(string s) => new(s);
        
        /// <summary>
        ///   Initializes the <see cref="ConfigPath"/> from a <see cref="string"/> value.
        /// </summary>
        /// <param name="stringValue">
        ///   The configuration path in its textual form. 
        /// </param>
        public ConfigPath(string stringValue) 
        : base(stringValue, ConfigDefaultSeparator)
        {
        }

        /// <summary>
        ///   Initializes the <see cref="ConfigPath"/> from a collection of string items,
        ///   each representing the an element of the configuration path, from root to leaf.
        /// </summary>
        /// <param name="items">
        ///   The configuration path elements.
        /// </param>
        public ConfigPath(string[] items)
        : base(items, ConfigDefaultSeparator)
        {
        }
    }
}