using System.Collections.Generic;

namespace TetraPak.XP.Serialization
{
    /// <summary>
    ///   Contains a key map and other mapping settings.
    /// </summary>
    public class KeyMapInfo
    {
        /// <summary>
        ///   Specifies whether to only include keys supported by the <see cref="Map"/>.
        /// </summary>
        public bool IsStrict { get; }

        /// <summary>
        ///   A dictionary where each key corresponds to an incoming attribute name and each
        ///   value the attribute name to be used instead.
        /// </summary>
        public IDictionary<string, string> Map { get; }
        
        public KeyMapInfo(IDictionary<string, string> map, bool isStrict)
        {
            Map = map;
            IsStrict = isStrict;
        }
    }
}