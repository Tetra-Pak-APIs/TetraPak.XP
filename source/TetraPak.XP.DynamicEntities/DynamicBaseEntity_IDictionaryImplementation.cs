using System.Collections;
using System.Collections.Generic;
#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace TetraPak.XP.DynamicEntities
{
    partial class DynamicEntity
    {
        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => _dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _dictionary).GetEnumerator();

        public void Add(KeyValuePair<string, object?> item) => Add(item.Key, item.Value);

        public void Clear() => _dictionary.Clear();

        public bool Contains(KeyValuePair<string, object?> item) => _dictionary.Contains(item);

        public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex)
        {
            foreach (var kvp in array)
            {
                _dictionary.Add(kvp.Key, kvp.Value);
            }
        }

        public bool Remove(KeyValuePair<string, object?> item) => _dictionary.Remove(item.Key);

        public int Count => _dictionary.Count;

        public bool IsReadOnly => _dictionary.IsReadOnly;

        public virtual void Add(string key, object? value) => SetValue(key, value);

        public bool ContainsKey(string key) => _dictionary.ContainsKey(key);

        public bool Remove(string key) => _dictionary.Remove(key);

#if NET5_0_OR_GREATER
        public virtual bool TryGetValue(string key, [MaybeNullWhen(false)] out object value) => _dictionary.TryGetValue(key, out value);
#else
        public virtual bool TryGetValue(string key, out object? value) => _dictionary.TryGetValue(key, out value);
#endif        

        public object? this[string key]
        {
            get
            {
                var outcome = OnTryGetPropertyValue<object>(key);
                return outcome
                    ? outcome.Value!
                    : GetValue<object>(key)!;
            }
            set
            {
                var o = OnTrySetPropertyValue(key, value);
                if (!o)
                {
                    SetValue(key, value);
                }
            }
        }

        public ICollection<string> Keys => _dictionary.Keys;

        public ICollection<object?> Values => _dictionary.Values;
    }
}