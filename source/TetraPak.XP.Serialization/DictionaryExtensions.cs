using System;
using System.Collections.Generic;
using System.Linq;

namespace TetraPak.XP.Serialization
{
    public static class DictionaryExtensions
    {
        /// <summary>
        ///   Generates a new <see cref="IDictionary{TKey,TValue}"/> where all keys are
        ///   renamed according to a specified key map.
        /// </summary>
        /// <param name="self">
        ///   The source dictionary. 
        /// </param>
        /// <param name="keyMap">
        ///   A dictionary containing the key mapping (keys=source keys, values=target keys).
        /// </param>
        /// <param name="isRestricted">
        ///   (optional; default=<c>false</c>)<br/>
        ///   Specifies whether to only include attributes whose keys can be found in the <paramref name="keyMap"/>.  
        /// </param>
        /// <typeparam name="TValue">
        ///   The dictionary's value <see cref="Type"/>.
        /// </typeparam>
        /// <returns>
        ///   A remapped dictionary.
        /// </returns>
        public static IDictionary<string, TValue> MapSafe<TValue>(
            this IDictionary<string, TValue> self, 
            KeyMapInfo keyMap,
            bool isRestricted = false) 
            =>
            self.ToArray().MapSafe(keyMap).ToDictionary();
        
        public static IEnumerable<KeyValuePair<string, TValue>> MapSafe<TValue>(
            this KeyValuePair<string, TValue>[] self,
            KeyMapInfo keyMap)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < self.Length; i++)
            {
                if (self[i].Value is null)
                    continue;
        
                var key = self[i].Key;
                if (keyMap.Map.TryGetValue(key, out var mappedKey))
                {
                    yield return new KeyValuePair<string, TValue>(mappedKey, self[i].Value);
                }
                else if (!keyMap.IsStrict)
                {
                    yield return new KeyValuePair<string, TValue>(self[i].Key, self[i].Value);
                }
            }
        }
    }
}