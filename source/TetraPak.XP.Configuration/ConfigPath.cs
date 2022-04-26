using System;
using System.Diagnostics;
using TetraPak.XP.DynamicEntities;

namespace TetraPak.XP.Configuration
{
    /// <summary>
    ///   String value, representing a configuration path.
    /// </summary>
    [DebuggerDisplay("{" + nameof(StringValue) + "}")]
    public sealed class ConfigPath : DynamicPath
    {
        /// <summary>
        ///   The default separator used in configuration paths.
        /// </summary>
        public const string ConfigDefaultSeparator = ":";

        /// <summary>
        ///   Implicit type cast <see cref="string"/> => <see cref="ConfigPath"/>.
        /// </summary>
        public static implicit operator ConfigPath(string s) => new(s);

        protected override DynamicPath OnCreate(string[] items) => new ConfigPath(items, Separator, Comparison);

        public new ConfigPath Push(params string[] items) => (ConfigPath)OnCreate(AddRange(items));

        public new ConfigPath Insert(params string[] items) => (ConfigPath)OnCreate(InsertRange(0, items));

        /// <summary>
        ///   Constructs and returns an empty <see cref="ConfigPath"/> value.
        /// </summary>
        public new static ConfigPath Empty => new(Array.Empty<string>());

        /// <summary>
        ///   Initializes the <see cref="ConfigPath"/> from a <see cref="string"/> value.
        /// </summary>
        /// <param name="stringValue">
        ///   The configuration path in its textual form. 
        /// </param>
        public ConfigPath(string? stringValue)
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
        /// <param name="separator">
        ///   (optional)<br/>
        ///   The separator token to be used in sting representations for the path. 
        /// </param>
        /// <param name="comparison">
        ///   (optional; default=<see cref="StringComparison.Ordinal"/>)<br/>
        ///   Specifies how to perform comparison for this path. 
        /// </param>
        public ConfigPath(string[] items, string? separator = null,
            StringComparison comparison = StringComparison.Ordinal)
            : base(items, separator ?? ConfigDefaultSeparator, comparison)
        {
        }
    }
}