using System;
using System.Collections.Generic;
using System.Linq;

namespace TetraPak.XP
{
    /// <summary>
    ///   Provides convenient helper methods for working with dictionaries.
    /// </summary>
    public static class DictionaryHelper
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
        public static IDictionary<string, TValue> Map<TValue>(
            this IDictionary<string, TValue> self, 
            IDictionary<string,string> keyMap,
            bool isRestricted = false) 
            =>
            self.ToArray().Map(keyMap, isRestricted).ToDictionary();

        public static IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> pairs) where TKey : notnull
        {
#if NET5_0_OR_GREATER
            return new Dictionary<TKey, TValue>(pairs);
#else

            return pairs.ToDictionary(pair => pair.Key, pair => pair.Value);
#endif
        }

        public static IEnumerable<KeyValuePair<string, TValue>> Map<TValue>(
            this KeyValuePair<string, TValue>[] self,
            IDictionary<string, string> keyMap, 
            bool isRestricted)
        {
            for (var i = 0; i < self.Length; i++)
            {
                var key = self[i].Key;
                var mappedKey = keyMap[key];
                yield return new KeyValuePair<string, TValue>(mappedKey, self[i].Value);
            }
        }
        
        /// <summary>
        ///   Generates an inverted version of a dictionary, making all values become keys and vice versa.
        ///   Please note that the key and value <see cref="Type"/> must be compatible.
        /// </summary>
        /// <param name="self">
        ///   A source dictionary. 
        /// </param>
        /// <typeparam name="T">
        ///   The key/value <see cref="Type"/>.
        /// </typeparam>
        /// <returns>
        ///   A new <see cref="IDictionary{TKey,TValue}"/> object.
        /// </returns>
        /// <seealso cref="ToInverted{T}(System.Collections.Generic.IEnumerable{KeyValuePair{T,T}})"/>
        public static IDictionary<T,T> ToInverted<T>(this IDictionary<T,T> self) where T : notnull 
            => ((IEnumerable<KeyValuePair<T, T>>) self).ToInverted<T,T>().ToDictionary();

        /// <summary>
        ///   Generates a collection of <see cref="KeyValuePair{T,T}"/> items from an existing collection
        ///   but inverts the position of the keys and values. 
        /// </summary>
        /// <param name="self">
        ///   A source collection of <see cref="KeyValuePair{TKey,TValue}"/>.
        /// </param>
        /// <typeparam name="T">
        ///   The key/value <see cref="Type"/> (must be the same).
        /// </typeparam>
        /// <returns>
        ///   A new collection of <see cref="KeyValuePair{TKey,TValue}"/> items.
        /// </returns>
        /// <seealso cref="ToInverted{T}(System.Collections.Generic.IDictionary{T,T})"/>
        public static IEnumerable<KeyValuePair<T,T>> ToInverted<T>(this IEnumerable<KeyValuePair<T,T>> self)
        {
            foreach (var pair in self)
            {
                yield return new KeyValuePair<T,T>(pair.Value, pair.Key);
            }
        }

        /// <summary>
        ///   Generates a <see cref="IDictionary{TKey,TValue}"/> by inverting the key/values,
        ///   making the values keys and keys values.
        /// </summary>
        /// <param name="self">
        ///   The source dictionary.
        /// </param>
        /// <typeparam name="TKey">
        ///   The source dictionary key type.
        /// </typeparam>
        /// <typeparam name="TValue">
        ///   The source dictionary value type.
        /// </typeparam>
        /// <returns>
        ///   A new dictionary.
        /// </returns>
        public static IDictionary<TValue, TKey> ToInverted<TKey, TValue>(this IDictionary<TKey, TValue> self) 
            where TValue : notnull where TKey : notnull 
        => new Dictionary<TValue, TKey>(ToInverted((IEnumerable<KeyValuePair<TKey, TValue>>)self).ToDictionary());

        /// <summary>
        ///   Generates a collection of <see cref="KeyValuePair{TKey,TValue}"/> items from a source
        ///   collection by inverting the key/values, turning keys into values and values into keys.
        /// </summary>
        /// <param name="self">
        ///   The source key value pair collection.
        /// </param>
        /// <typeparam name="TKey">
        ///   The source key/value pair collection key type.
        /// </typeparam>
        /// <typeparam name="TValue">
        ///   The source key/value pair collection value type.
        /// </typeparam>
        /// <returns>
        ///   A new dictionary.
        /// </returns>
        /// <remarks>
        ///   Please note that all key-value pairs where the value is <c>null</c> will be ignored.
        /// </remarks>
        public static IEnumerable<KeyValuePair<TValue,TKey>> ToInverted<TKey,TValue>(this IEnumerable<KeyValuePair<TKey,TValue>> self)
        {
            foreach (var pair in self)
            {
                if (pair.Value is null)
                    continue;
                
                yield return new KeyValuePair<TValue, TKey>(pair.Value, pair.Key);
            }
        }

        /// <summary>
        ///   Maps all key/value elements to a collection of items of a specified type.
        /// </summary>
        public static IEnumerable<T> MapTo<T, TKey, TValue>(
            this IDictionary<TKey, TValue> self,
            Func<KeyValuePair<TKey, TValue>, T> mapper) 
        where TKey : notnull
        {
            return self.Select(mapper);
        }    
    }
}