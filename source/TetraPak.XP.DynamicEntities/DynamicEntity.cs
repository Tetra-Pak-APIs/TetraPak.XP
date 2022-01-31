using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using TetraPak.XP.Logging;
using TetraPak.XP.Serialization;

namespace TetraPak.XP.DynamicEntities
{
    [Serializable]
    //[JsonConverter(typeof(DynamicEntityJsonConverter<DynamicEntity>))]
    public partial class DynamicEntity : IDictionary<string,object?> 
    {
        IDictionary<string, object?> _dictionary = new Dictionary<string, object?>();
        KeyTransformationFormat? _jsonKeyFormat;

        public static KeyTransformationFormat DefaultKeyTransformationFormat { get; set; }

        /// <summary>
        ///   Gets or sets the JSON key format used for all values. 
        /// </summary>
        public KeyTransformationFormat JsonKeyFormat
        {
            get => _jsonKeyFormat ?? DefaultKeyTransformationFormat;
            set => _jsonKeyFormat = value;
        }

#if DEBUG
        static int s_counter;

        [JsonIgnore]
        public int DebugInstanceId { get; } = ++s_counter;
#endif

        protected Dictionary<string, object> GetDictionary() => (Dictionary<string, object>) _dictionary;
        
        protected void SetDictionary(IDictionary<string, object?> dictionary) => _dictionary = dictionary;
        
        [DebuggerStepThrough]
        public virtual TValue? Get<TValue>(TValue? useDefault = default, [CallerMemberName] string? caller = null) 
            => GetValue(JsonKey(caller!), useDefault);

        public virtual TValue? GetValue<TValue>(string key, TValue? useDefault = default)
        {
            if (!_dictionary.TryGetValue(key, out var obj)) 
                return useDefault;

            return obj switch
            {
                TValue castValue => castValue,
                null => typeof(TValue).IsGenericBase(typeof(Nullable<>))
                    ? TypeHelper.GetDefaultValue<TValue>()
                    : useDefault,
                _ => throw new Exception($"{key} cannot be cast to {typeof(TValue)} (was {obj.GetType()})")
            };
        }

        static Outcome<T> failItemNotFound<T>(string key) => Outcome<T>.Fail(new Exception($"Item not found: {key}"));

        protected virtual Outcome<TValue> OnTryGetPropertyValue<TValue>(string key)
        {
            var property = GetPropertyWithJsonPropertyName(key);
            if (property is null || !property.CanRead)
                return failItemNotFound<TValue>(key);

            var value = property.GetValue(this);
            if (value is not TValue castValue)
                return Outcome<TValue>.Fail(new InvalidCastException($"{key} cannot be cast to {typeof(TValue)}"));
            
            return Outcome<TValue>.Success(castValue);
        }

        protected virtual Outcome<TValue> OnTrySetPropertyValue<TValue>(string key, TValue value)
        {
            var property = GetPropertyWithJsonPropertyName(key);
            if (property is null || !property.CanWrite)
                return failItemNotFound<TValue>(key);

            try
            {
                property.SetValue(this, value);
                var o = OnTryGetPropertyValue<TValue>(key);
                return o ? o : failItemNotFound<TValue>(key);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public PropertyInfo? GetPropertyWithJsonPropertyName(string key)
        {
            PropertyInfo? namedCandidate = null;
            var propertyInfo = GetType().GetProperties()
                .FirstOrDefault(p =>
                {
                    var attr = p.GetCustomAttribute<JsonPropertyNameAttribute>();
                    if (attr?.Name.Equals(key) ?? false)
                        return true;

                    if (p.Name.ToLowerInitial().Equals(key))
                        namedCandidate = p;

                    return false;
                });
            return propertyInfo ?? namedCandidate;
        }

        /// <summary>
        ///   Sets a (property) value.
        /// </summary>
        /// <param name="value">
        ///   The value to be assigned.
        /// </param>
        /// <param name="caller">
        ///   The name of the caller (presumable a property name).
        /// </param>
        /// <typeparam name="TValue">
        ///   The type of value to be set.
        /// </typeparam>
        public virtual void Set<TValue>(TValue value, [CallerMemberName] string? caller = null)
            => SetValue(JsonKey(caller!), value);

        public virtual void SetValue<TValue>(string key, TValue? value) => _dictionary[key] = convertDeserializedArray(key, value);

        protected void SetRaw(string key, object value) => _dictionary[key] = convertDeserializedArray(key, value);

        protected object? GetRaw(string key) => _dictionary.TryGetValue(key, out var value) ? value : null;

        object? convertDeserializedArray(string key, object? value)
        {
            if (value is not Array array)
                return value;
            
            var property = GetPropertyWithJsonPropertyName(key);
            if (property is null)
                return value;

            if (property.PropertyType.IsGenericBase(typeof(IEnumerable<>)))
            {
                var itemType = property.PropertyType.GenericTypeArguments[0];
                var listType = typeof(List<>).MakeGenericType(itemType);
                var list = (IList) Activator.CreateInstance(listType)!;
                foreach (var item in array)
                {
                    list.Add(item);
                }

                return list;
            }

            return value;
        }

        public DynamicEntity ApplyFrom(object source) => onApplyFrom(source);

        DynamicEntity onApplyFrom(object source)
        {
            var assigned = new HashSet<string>();
            if (source is IDictionary<string, object> dictionary)
                onApplyFromDictionary(dictionary, assigned);

            var sourceProps = source.GetType().GetProperties().Where(pi => pi.CanRead).ToArray();
            for (var i = 0; i < sourceProps.Length; i++)
            {
                var targetProp = GetType().GetProperty(sourceProps[i].Name);
                if (targetProp is null || !targetProp.CanWrite || targetProp.IsIndexer() || assigned.Contains(targetProp.Name))
                    continue;

                targetProp.SetValue(this, sourceProps[i].GetValue(source));
            }

            return this;
        }

        void onApplyFromDictionary(IDictionary<string, object> source, ISet<string> assigned)
        {
            var isDynamicEntity = source is DynamicEntity;
            foreach (var pair in source)
            {
                SetValue(pair.Key, pair.Value);
                if (isDynamicEntity)
                    assigned.Add(pair.Key);
            }
        }
        
        // todo Make into extension method
        public static TEntity? FromJson<TEntity>(string json) 
        where TEntity : DynamicEntity
        {
            var entity = JsonSerializer.Deserialize<TEntity>(json);
            return entity;
        }

        public static object? FromJson(string json, Type returnType)
        {
            if (!typeof(DynamicEntity).IsAssignableFrom(returnType))
                throw new InvalidCastException($"Cannot deserialize type {returnType}");
            
            return JsonSerializer.Deserialize(json, returnType);
        }

        public string ToJson(bool indented = false, IEnumerable<string>? ignoreKeys = null) 
            => ToJson(new JsonSerializerOptions 
                { 
                    WriteIndented = indented,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }, 
                ignoreKeys);

        public string ToJson(JsonSerializerOptions options, IEnumerable<string>? ignoreKeys = null)
        {
            if (ignoreKeys is {})
            {
                removeKeys(ignoreKeys); 
            }
            return JsonSerializer.Serialize(this, options);  
        }

        void removeKeys(IEnumerable<string> keys)
        {
            foreach (var key in keys)
            {
                if (_dictionary.ContainsKey(key))
                {
                    _dictionary.Remove(key);
                }
            }
        }

        void pruneKeys(IEnumerable<string> protectedKeys)
        {
            var hash = new HashSet<string>(protectedKeys);
            var keys = _dictionary.Keys.ToArray();
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < keys.Length; i++)
            {
                var key = keys[i];
                if (hash.Contains(key))
                    continue;
                
                _dictionary.Remove(key);
            }
        }

        public virtual TEntity Clone<TEntity>(ILog? log = null) where TEntity : DynamicEntity
        {
            var json = ToJson(true);
            try
            {
                var entity = (TEntity) FromJson<TEntity>(json)!.ObjectifyJsonElements();
                return entity;
            }
            catch (Exception ex)
            {
                if (log is null)
                    throw;

                using var section = log.Section(LogRank.Debug, $"Error while cloning {this} ({typeof(TEntity)}):");
                section.Error(ex, "Dumping troublesome JSON ...");
                section.Debug(json);
                throw;
            }
        }

        public object ObjectifyJsonElements() 
        {
            if (!_dictionary.Any(pair => pair.Value is JsonElement))
                return this;

            var target = new Dictionary<string, object?>();
            foreach (var pair in this)
            {
                var property = GetPropertyWithJsonPropertyName(pair.Key);
                if (!(pair.Value is JsonElement jsonElement) || property is null)
                {
                    target[pair.Key] = pair.Value;
                    continue;
                }

                var value = jsonElement.ToObject(property.PropertyType);
                target[pair.Key] = value;
                if (value is DynamicEntity entity)
                {
                    entity.ObjectifyJsonElements();
                }
                else if (value.IsCollectionOf<DynamicEntity>(out var items))
                {
                    foreach (DynamicEntity? entityItem in items)
                    {
                        entityItem?.ObjectifyJsonElements();
                    }
                }
            }

            _dictionary = target;
            return this;
        }

        protected string JsonKey([CallerMemberName] string?caller = null)
        {
            return caller!.ToJsonKeyFormat(JsonKeyFormat);
        }
        
        public virtual object Clone(Type returnType)
        {
            return FromJson(ToJson(), returnType)!;
        }
    }
}