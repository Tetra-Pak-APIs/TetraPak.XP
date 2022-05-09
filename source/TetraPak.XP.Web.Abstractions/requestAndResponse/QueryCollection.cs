using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using MsStringValues=Microsoft.Extensions.Primitives.StringValues;

namespace TetraPak.XP.Web.Abstractions
{
/// <summary>
    /// The HttpRequest query string collection
    /// </summary>
    public sealed class QueryCollection : IQueryCollection
    {
        public static readonly QueryCollection s_empty = new();
        static readonly string[] s_emptyKeys = Array.Empty<string>();
        static readonly MsStringValues[] s_emptyValues = Array.Empty<MsStringValues>();

        static readonly Enumerator s_emptyEnumerator = new();
        // Pre-box
        static readonly IEnumerator<KeyValuePair<string, MsStringValues>> s_emptyIEnumeratorType = s_emptyEnumerator;
        static readonly IEnumerator s_emptyIEnumerator = s_emptyEnumerator;

        Dictionary<string, MsStringValues>? Store { get; set; }

        /// <summary>
        /// Get or sets the associated value from the collection as a single string.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <returns>the associated value from the collection as a StringValues or StringValues.Empty if the key is not present.</returns>
        public MsStringValues this[string key]
        {
            get
            {
                if (Store == null)
                {
                    return MsStringValues.Empty;
                }

                if (TryGetValue(key, out var value))
                {
                    return value;
                }
                return MsStringValues.Empty;
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="QueryCollection" />;.
        /// </summary>
        /// <returns>The number of elements contained in the <see cref="QueryCollection" />.</returns>
        public int Count => Store?.Count ?? 0;

        public ICollection<string> Keys
        {
            get
            {
                if (Store == null)
                {
                    return s_emptyKeys;
                }
                return Store.Keys;
            }
        }

        /// <summary>
        /// Determines whether the <see cref="QueryCollection" /> contains a specific key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>true if the <see cref="QueryCollection" /> contains a specific key; otherwise, false.</returns>
        public bool ContainsKey(string key)
        {
            if (Store == null)
            {
                return false;
            }
            return Store.ContainsKey(key);
        }

        /// <summary>
        /// Retrieves a value from the collection.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>true if the <see cref="QueryCollection" /> contains the key; otherwise, false.</returns>
        public bool TryGetValue(string key, out MsStringValues value)
        {
            if (Store == null)
            {
                value = default(MsStringValues);
                return false;
            }
            return Store.TryGetValue(key, out value);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="Enumerator" /> object that can be used to iterate through the collection.</returns>
        public Enumerator GetEnumerator()
        {
            if (Store == null || Store.Count == 0)
            {
                // Non-boxed Enumerator
                return s_emptyEnumerator;
            }
            return new Enumerator(Store.GetEnumerator());
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="IEnumerator{T}" /> object that can be used to iterate through the collection.</returns>
        IEnumerator<KeyValuePair<string, MsStringValues>> IEnumerable<KeyValuePair<string, MsStringValues>>.GetEnumerator()
        {
            if (Store == null || Store.Count == 0)
            {
                // Non-boxed Enumerator
                return s_emptyIEnumeratorType;
            }
            return Store.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            if (Store == null || Store.Count == 0)
            {
                // Non-boxed Enumerator
                return s_emptyIEnumerator;
            }
            return Store.GetEnumerator();
        }

        public struct Enumerator : IEnumerator<KeyValuePair<string, MsStringValues>>
        {
            // Do NOT make this readonly, or MoveNext will not work
            private Dictionary<string, MsStringValues>.Enumerator _dictionaryEnumerator;
            private bool _notEmpty;

            internal Enumerator(Dictionary<string, MsStringValues>.Enumerator dictionaryEnumerator)
            {
                _dictionaryEnumerator = dictionaryEnumerator;
                _notEmpty = true;
            }

            public bool MoveNext()
            {
                if (_notEmpty)
                {
                    return _dictionaryEnumerator.MoveNext();
                }
                return false;
            }

            public KeyValuePair<string, MsStringValues> Current => _notEmpty ? _dictionaryEnumerator.Current : default;

            public void Dispose()
            {
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            void IEnumerator.Reset()
            {
                if (_notEmpty)
                {
                    ((IEnumerator)_dictionaryEnumerator).Reset();
                }
            }
        }
        
        public QueryCollection()
        {
        }

        public QueryCollection(Dictionary<string, MsStringValues> store)
        {
            Store = store;
        }

        public QueryCollection(QueryCollection store)
        {
            Store = store.Store;
        }

        public QueryCollection(int capacity)
        {
            Store = new Dictionary<string, MsStringValues>(capacity, StringComparer.OrdinalIgnoreCase);
        }
    }
}