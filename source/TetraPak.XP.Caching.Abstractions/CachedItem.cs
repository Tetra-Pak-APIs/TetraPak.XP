using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TetraPak.XP.Caching.Abstractions
{
    public struct CachedItem<TValue>
    {
        internal DateTime ExpiresUtc { get; }
        public string Path { get; set; }
        public bool IsExpired => DateTime.Now >= ExpiresUtc;
        public TValue Value { get; }

        public CachedItem(string path, TValue value, DateTime expiresUtc)
        {
            Path = path;
            ExpiresUtc = expiresUtc;
            Value = value;
        }
    }

    public class CachedItemDTO
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("expires")]
        public string Expires { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public static class CachedItemHelper
    {
        internal static CachedItem<T> FromJson<T>(this string json)
        {
            var dto = JsonSerializer.Deserialize<CachedItemDTO>(json);
            var value = JsonSerializer.Deserialize<T>(dto.Value);
            var expires = DateTime.ParseExact(dto.Expires, "O", CultureInfo.InvariantCulture);
            return new CachedItem<T>(dto.Key, value, expires);
        }

        internal static string ToJson<T>(this CachedItem<T> item)
        {
            var dto = new CachedItemDTO
            {
                Key = item.Path,
                Expires = item.ExpiresUtc.ToString("O"),
                Value = JsonSerializer.Serialize(item.Value)
            };
            return JsonSerializer.Serialize(dto);
        }
    }
}