using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Serialization;

namespace TetraPak.XP.DynamicEntities
{
    static class JsonConversion
    {
        [ThreadStatic]
        static JsonConvertDynamicEntitiesAttribute? s_convertArbitraryObjects;
        
        internal static JsonConvertDynamicEntitiesAttribute? GetConvertArbitraryObjects(Type typeToConvert) => s_convertArbitraryObjects ??= typeToConvert.GetCustomAttribute<JsonConvertDynamicEntitiesAttribute>();
    }
    
    public class DynamicEntityJsonConverter<T> : JsonConverter<T> 
    where T : DynamicEntity
    {
        JsonConvertDynamicEntitiesAttribute? _convertArbitraryObjects;
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
            _convertArbitraryObjects = JsonConversion.GetConvertArbitraryObjects(typeToConvert);
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException($"Cannot de-serialize {typeof(T)}. Expected start of object.");
            
            var entity = OnConstructRootEntity(ref reader, options);
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
                        ? OnDebugReadPropertyValue(key, ref reader, entity, options) 
                        : OnReadPropertyValue(key, ref reader, entity, options);
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

        protected virtual T OnConstructRootEntity(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            return XpServices.Get<T>() ?? Activator.CreateInstance<T>();
        }

        protected virtual DynamicEntity OnConstructEntity(string? key, ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (_convertArbitraryObjects is null)
                return JsonSerializer.Deserialize<DynamicEntity>(ref reader, options);

            if (_convertArbitraryObjects.Factory is { })
                return _convertArbitraryObjects.Factory.DeserializeEntity(key, ref reader);

            return _convertArbitraryObjects.All is { }
                ? (DynamicEntity) JsonSerializer.Deserialize(ref reader, _convertArbitraryObjects.All)
                : JsonSerializer.Deserialize<DynamicEntity>(ref reader, options);
        }

        protected virtual object? OnReadPropertyValue(string key, ref Utf8JsonReader reader, T entity, JsonSerializerOptions options)
        {
#if DEBUG
            if (IsDebugging)
                return OnDebugReadPropertyValue(key, ref reader, entity, options);
#endif            
            if (reader.TokenType == JsonTokenType.Null)
                return null;
            
            var property = entity.GetPropertyWithJsonPropertyName(key);
            var implementedAs = property?.GetCustomAttribute<ImplementedAsAttribute>();
            var propertyType = implementedAs?.Type ?? property?.PropertyType;

            return propertyType != null
                ? OnReadValueFromPropertyType(key, ref reader, propertyType, options)
                : OnReadValueFromInferredTokenType(key, ref reader, options);
        }

        protected virtual object? OnDebugReadPropertyValue(string key, ref Utf8JsonReader reader, T entity, JsonSerializerOptions options)
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
                    ? OnReadValueFromPropertyType(key, ref reader, propertyType, options)
                    : OnReadValueFromInferredTokenType(key, ref reader, options);

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
            string key,
            ref Utf8JsonReader reader, 
            Type propertyType, 
            JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize(ref reader, propertyType, options);
        }
        
        [SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
        protected virtual object? OnReadValueFromInferredTokenType(
            string key,
            ref Utf8JsonReader reader, 
            JsonSerializerOptions options)
        {
            if (!reader.Read())
                throw new SerializationException();

            JsonTokenType? inferredType = null; 
            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                    validateTokenType(reader, JsonTokenType.StartObject);
                    return OnConstructEntity(key, ref reader, options);
                
                case JsonTokenType.StartArray: 
                    validateTokenType(reader, JsonTokenType.StartArray);
                    return deserializeEntityArray(key, ref reader, options);

                case JsonTokenType.String:
                    validateTokenType(reader, JsonTokenType.String);
                    return reader.GetString();
                    
                case JsonTokenType.Number:
                    validateTokenType(reader, JsonTokenType.Number);
                    return reader.GetDouble();

                case JsonTokenType.True:
                    validateTokenType(reader, JsonTokenType.True);
                    return true;
                
                case JsonTokenType.False:
                    validateTokenType(reader, JsonTokenType.True);
                    return false;
                
                case JsonTokenType.Null:
                    return null;
                
                default:
                    return readValueFromInferredValue(ref reader);
            }

            void validateTokenType(Utf8JsonReader rdr, JsonTokenType expectedType)
            {
                expectedType = expectedType == JsonTokenType.False ? JsonTokenType.True : expectedType;
                if (inferredType.HasValue && inferredType != expectedType)
                    throw new JsonException(
                        $"Unexpected token type in array at {rdr.Position}: {rdr.TokenType} (expected {expectedType})");

                inferredType = expectedType;
            }
        }

        object deserializeEntityArray(string key, ref Utf8JsonReader reader,
            JsonSerializerOptions options)
        {
            var initialNullValues = 0;
            while (true)
            {
                if (!reader.Read()) 
                    throw new SerializationException($"Cannot read end of array at {reader.Position}");

                if (reader.TokenType == JsonTokenType.EndArray)
                    return initialNullValues == 0
                        ? Array.Empty<object>()
                        : Collection.ArrayOf<object>(initialNullValues, _ => null!);
                
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject:
                        var list = Collection.ListOf<DynamicEntity>(null!, initialNullValues);
                        do
                        {
                            var item = OnConstructEntity(key, ref reader, options);
                            list.Add(item);

                        } while (reader.Read() && reader.TokenType != JsonTokenType.EndArray);
                        return list.ToArray();

                    case JsonTokenType.String:
                        var strings = Collection.ListOf<string>(initialNullValues, _ => null!);
                        do
                        {
                            strings.Add(reader.GetString()!);
                            
                        } while (reader.Read() && reader.TokenType != JsonTokenType.EndArray);
                        return strings.ToArray();

                    case JsonTokenType.Number:
                        if (initialNullValues != 0)
                            throw new JsonException($"Unexpected number in array of objects at {reader.Position}");
                            
                        var numbers = new List<double>();
                        do
                        {
                            numbers.Add(reader.GetDouble());
                            
                        } while (reader.Read() && reader.TokenType != JsonTokenType.EndArray);
                        return numbers.ToArray();

                    case JsonTokenType.True:
                    case JsonTokenType.False:
                        if (initialNullValues != 0)
                            throw new JsonException($"Unexpected number in array of objects at {reader.Position}");

                        var booleans = new List<bool>();
                        do
                        {
                            booleans.Add(reader.GetBoolean());
                            
                        } while (reader.Read() && reader.TokenType != JsonTokenType.EndArray);
                        return booleans.ToArray();

                    case JsonTokenType.Null:
                        initialNullValues++;
                        continue;
                    
                    default:
                        throw new FormatException($"Unexpected token at {reader.Position}");
                }
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