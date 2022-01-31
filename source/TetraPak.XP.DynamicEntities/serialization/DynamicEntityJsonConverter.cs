using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using TetraPak.XP.Serialization;

namespace TetraPak.XP.DynamicEntities.Serialization
{
    public class DynamicEntityJsonConverter<T> : JsonConverter<T> 
    where T : DynamicEntity
    {
        HashSet<string>? _writeIgnoredProperties;

#if DEBUG
        public bool IsDebugging { get; private set; }
#endif
        
        protected string? LastId { get; set; }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
#if DEBUG
            IsDebugging = typeToConvert.GetCustomAttribute<DebugJsonConversionAttribute>() is { };
#endif            
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException($"Cannot de-serialize {typeof(T)}. Expected start of object.");
            
            var entity = Activator.CreateInstance<T>();
            var keyMap = entity is ISerializationKeyMapProvider keyMapProvider ? keyMapProvider.GetKeyMap() : null;
            var key = string.Empty;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return entity;
                
                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException(
                        $"Cannot de-serialize {typeof(T)}. Expected property name but found {reader.TokenType} (previous key was: '{key}')");

                key = reader.GetString();
                try
                {
                    if (key is null)
                        continue;
#if DEBUG
                    var value = IsDebugging 
                        ? OnDebugReadPropertyValue(ref reader, key, entity, options) 
                        : OnReadPropertyValue(ref reader, key, entity, options);
#else
                    var value = OnReadPropertyValue(ref reader, key, entity, options);
#endif
                    if (keyMap?.Map is {} && keyMap.Map.TryGetValue(key, out var mappedKey))
                    {
                        key = mappedKey;
                        entity.SetValue(mappedKey, value);
                        continue;
                    }

                    if (!keyMap?.IsStrict ?? true)
                    {
                        entity.SetValue(key, value);
                    }
                }
                catch (Exception ex)
                {
                    WriteError(ex, $"While reading key: '{key}'");
                    throw;
                }
            }

            return entity;
        }

        protected virtual object? OnReadPropertyValue(ref Utf8JsonReader reader, string key, T entity, JsonSerializerOptions options)
        {
#if DEBUG
            if (IsDebugging)
                return OnDebugReadPropertyValue(ref reader, key, entity, options);
#endif            
            if (reader.TokenType == JsonTokenType.Null)
                return null;
            
            var property = entity.GetPropertyWithJsonPropertyName(key);
            var implementedAs = property?.GetCustomAttribute<ImplementedAsAttribute>();
            var propertyType = implementedAs?.Type ?? property?.PropertyType;

            return propertyType != null
                ? OnReadValueFromPropertyType(ref reader, propertyType, key, options)
                : OnReadValueFromInferredTokenType(ref reader, key, options);
        }

        protected virtual object? OnDebugReadPropertyValue(ref Utf8JsonReader reader, string key, T entity, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;
            
            var property = entity.GetPropertyWithJsonPropertyName(key);
            var implementedAs = property?.GetCustomAttribute<ImplementedAsAttribute>();
            var propertyType = implementedAs?.Type ?? property?.PropertyType;
            if (propertyType is null)
            {
                WriteInfo($"Property not found: '{key}'");
            }

            try
            {
                var value= propertyType != null
                    ? OnReadValueFromPropertyType(ref reader, propertyType, key, options)
                    : OnReadValueFromInferredTokenType(ref reader, key, options);

                if (key == "id")
                {
                    var id = value?.ToString() ?? throw new Exception($"Value with key \"{key}\" returned a null id");
                    LastId = id;
                }
            
                if (propertyType is { })
                {
                    WriteInfo($"Read {key} = {value ?? "null"}");
                }

                return value;
            }
            catch (Exception ex)
            {
                WriteError(ex, $"When reading value for '{key}':");
                WriteInfo(ex.ToString());
                throw;
            }
        }

        protected virtual object? OnReadValueFromPropertyType(
            ref Utf8JsonReader reader, 
            Type propertyType, 
            string key,
            JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize(ref reader, propertyType, options);
        }
        
        [SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
        protected virtual object? OnReadValueFromInferredTokenType(
            ref Utf8JsonReader reader, 
            string key, 
            JsonSerializerOptions options)
        {
            if (!reader.Read())
                throw new SerializationException();
                
            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                    return JsonSerializer.Deserialize<object>(ref reader, options);
                
                case JsonTokenType.StartArray:
                    return JsonSerializer.Deserialize<object>(ref reader, options);

                case JsonTokenType.String:
                    return reader.GetString();
                    
                case JsonTokenType.Number:
                    return reader.GetDouble();

                case JsonTokenType.True:
                    return true;
                
                case JsonTokenType.False:
                    return false;
                
                case JsonTokenType.Null:
                    return null;
                
                default:
                    return readValueFromInferredValue(ref reader);
            }
        }

        static object readValueFromInferredValue(ref Utf8JsonReader reader) => reader.Read();

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (var pair in value)
            {
                var key = pair.Key;
                if (isWriteIgnored(value.GetType(), key))
                    continue;

                var obj = pair.Value;
                if (obj is null) // todo Try and figure out why compiler warns that this value "is always"
                {
                    writer.WriteNull(key);
                    continue;
                }
                writer.WritePropertyName(key);
                JsonSerializer.Serialize(writer, obj);
            }
            writer.WriteEndObject();
        }

        bool isWriteIgnored(MemberInfo type, string key)
        {
            if (_writeIgnoredProperties is {})
                return _writeIgnoredProperties.Contains(key);

            type = type.DeclaringType ?? type;
            
            _writeIgnoredProperties = new HashSet<string>();
            var ignoredAttr = type.GetCustomAttribute<SerializeIgnorePropertiesAttribute>();
            if (ignoredAttr is null)
                return false;
            
            foreach (var propertyName in ignoredAttr.PropertyNames)
            {
                _writeIgnoredProperties.Add(propertyName);
            }

            if (!char.IsLower(key[0]))
                key = key.ToLowerInitial();
            
            return _writeIgnoredProperties.Contains(key);
        }
        
        protected static void WriteInfo(string message) => Debug.WriteLine($"JSON --> {message}");

        protected void WriteError(Exception error, string message) => WriteInfo($" ERROR --> (Last id={LastId}) {message}\n{error}");

        // ReSharper disable once EmptyConstructor
        public DynamicEntityJsonConverter()
        {
        }
    }
}