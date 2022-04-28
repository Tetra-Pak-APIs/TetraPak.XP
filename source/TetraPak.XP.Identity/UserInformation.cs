using System;
using System.Collections.Generic;
using System.Linq;

namespace TetraPak.XP.Identity
{
    /// <summary>
    ///   Represents user information claims as a dictionary.
    /// </summary>
    public class UserInformation 
    {
        readonly IDictionary<string, string> _dictionary;

        /// <summary>
        ///   The names of all keys (types of claims).
        /// </summary>
        public string[] Keys => _dictionary.Keys.ToArray();

        /// <summary>
        ///   Tries to obtain user information value.
        /// </summary>
        /// <param name="key">
        ///   Identifies the value.
        /// </param>
        /// <param name="value">
        ///   Passes back the value when fund; otherwise <c>null</c>.
        /// </param>
        /// <typeparam name="T">
        ///   Specifies the type of value.
        /// </typeparam>
        /// <returns>
        ///   <c>true</c> if value was found; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="InvalidCastException">
        ///   Value was found but was not of specified type. 
        /// </exception>
        public bool TryGet<T>(string key, out T? value)
        {
            if (!_dictionary.TryGetValue(key, out var obj))
            {
                value = default;
                return false;
            }

            if (obj is not T typedValue) 
                throw new InvalidCastException();
            
            value = typedValue;
            return true;

            // todo Cast from Json Token to requested value.
            // todo Also replace Json Token with converted value to avoid converting twice
        }

        /// <summary>
        ///   Returns the user information as a <see cref="IDictionary{TKey,TValue}"/>.
        /// </summary>
        public IDictionary<string, string> ToDictionary() => _dictionary;

        internal UserInformation(IDictionary<string, string> dictionary)
        {
            _dictionary = dictionary;
        }
    }
}