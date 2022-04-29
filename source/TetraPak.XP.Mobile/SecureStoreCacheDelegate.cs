using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TetraPak.XP.Caching;
using TetraPak.XP.Caching.Abstractions;
using TetraPak.XP.DynamicEntities;
using TetraPak.XP.Logging.Abstractions;
using Xamarin.Essentials;

namespace TetraPak.XP.Mobile
{
    class SecureStoreCacheDelegate : SimpleCacheDelegate
    {
        readonly ITimeLimitedRepositories _repositories;

        public override Task<Outcome> CreateAsync(ITimeLimitedRepositoryEntry entry, bool strict)
        {
            return writeAsync(entry, () => base.CreateAsync(entry, strict));
        }

        public override Task<Outcome> UpdateAsync(ITimeLimitedRepositoryEntry entry, bool strict)
        {
            return writeAsync(entry, () => base.UpdateAsync(entry, strict));
        }

        public override Task<Outcome> CreateOrUpdateAsync(ITimeLimitedRepositoryEntry entry, bool strict)
        {
            return writeAsync(entry, () => base.CreateOrUpdateAsync(entry, strict));
        }

        async Task<Outcome> writeAsync(ITimeLimitedRepositoryEntry entry, Func<Task<Outcome>> baseHandler)
        {
            try
            {
                var serializedOutcome = await EntryDto.SerializeAsync(entry);
                if (!serializedOutcome)
                    return serializedOutcome;

                await SecureStorage.SetAsync(entry.Path, serializedOutcome.Value!);
                return await baseHandler();
            }
            catch (Exception ex)
            {
                return Outcome.Fail(ex);
            }
        }

        public override async Task<Outcome> DeleteAsync(DynamicPath path, bool strict)
        {
            try
            {
                if (!SecureStorage.Remove(path))
                    return Outcome.Fail($"Could not remove cached entry: '{path}'");
                
                return await base.DeleteAsync(path, strict);
            }
            catch (Exception ex)
            {
                return Outcome.Fail(ex);
            }        
        }

        public override async Task<Outcome<ITimeLimitedRepositoryEntry>> ReadRawEntryAsync(DynamicPath path)
        {
            try
            {
                var outcome = await base.ReadRawEntryAsync(path);
                if (outcome)
                    return outcome;
                
                var serialized = await SecureStorage.GetAsync(path);
                return await EntryDto.DeserializeAsync(serialized, _repositories);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        // async Task<Outcome<string>> serializeAsync(ITimeLimitedRepositoryEntry entry)
        // {
        //     return await EntryDto.Serialize(entry);
        // }

        public SecureStoreCacheDelegate(ITimeLimitedRepositories repositories, ILog? log) 
        : base(log)
        {
            _repositories = repositories;
        }

        class EntryDto
        {
            const string DateTimeFormat =  "yyyy-MM-ddTHH:mm:ss.fffff";

            [JsonPropertyName("path")]
            public string Path { get; set;  }

            [JsonPropertyName("spawnTimeUtc")]
            public string SpawnTimeUtc { get; set; }

            [JsonPropertyName("customLifeSpan")]
            public string? CustomLifeSpan { get; set; }

            [JsonPropertyName("customMaxLifeSpan")]
            public string? CustomMaxLifeSpan { get; set; }

            [JsonPropertyName("value")]
            public string Value { get; set; }

            [JsonPropertyName("typeFullName")]
            public string TypeFullName { get; set; }

            public static Task<Outcome<string>> SerializeAsync(ITimeLimitedRepositoryEntry entry)
            {
                var value = entry.GetValue();
                var serializedValue = JsonSerializer.Serialize(value);
                var spawnRimeUtc = entry.SpawnTimeUtc.ToString("O");
                try
                {
                    var dto = new EntryDto(entry.Path, spawnRimeUtc, serializedValue, value.GetType().FullName)
                    {
                        CustomLifeSpan = entry is SimpleCacheEntry { CustomLifeSpan: { } } simpleEntry1 
                            ? simpleEntry1.CustomLifeSpan.Value.Ticks.ToString()
                            : null,
                        CustomMaxLifeSpan = entry is SimpleCacheEntry { CustomMaxLifeSpan: { } } simpleEntry2 
                            ? simpleEntry2.CustomMaxLifeSpan.Value.Ticks.ToString()
                            : null,
                    };
                    var serialized = JsonSerializer.Serialize(dto);
                    return Task.FromResult(Outcome<string>.Success(serialized));
                }
                catch (Exception ex)
                {
                    return Task.FromResult(Outcome<string>.Fail(ex));
                }
            }

            public static Task<Outcome<ITimeLimitedRepositoryEntry>> DeserializeAsync(
                string serializedEntryDto, 
                ITimeLimitedRepositories repositories)
            {
                try
                {
                    var entryDto = JsonSerializer.Deserialize<EntryDto>(serializedEntryDto)!;
                    if (!DateTime.TryParseExact(entryDto.SpawnTimeUtc, DateTimeFormat, null, DateTimeStyles.None, out var spawnTimeUtc))
                        return Task.FromResult(Outcome<ITimeLimitedRepositoryEntry>.Fail(
                            $"Illegal date time format specified for {nameof(ITimeLimitedRepositoryEntry.SpawnTimeUtc)}: '{entryDto.SpawnTimeUtc}'"));

                    TimeSpan? customLifeSpan = null;
                    if (entryDto.CustomLifeSpan is { })
                    {
                        if (!long.TryParse(entryDto.CustomLifeSpan, out var ticks))
                            return Task.FromResult(Outcome<ITimeLimitedRepositoryEntry>.Fail(
                                $"Illegal timespan format specified for {nameof(SimpleCacheEntry.CustomLifeSpan)}: '{entryDto.CustomLifeSpan}'"));
                        
                        customLifeSpan = TimeSpan.FromTicks(ticks);
                    }

                    TimeSpan? customMaxLifeSpan = null;
                    if (entryDto.CustomLifeSpan is { })
                    {
                        if (!long.TryParse(entryDto.CustomLifeSpan, out var ticks))
                            return Task.FromResult(Outcome<ITimeLimitedRepositoryEntry>.Fail(
                                $"Illegal timespan format specified for {nameof(SimpleCacheEntry.CustomMaxLifeSpan)}: '{entryDto.CustomMaxLifeSpan}'"));
                        
                        customMaxLifeSpan = TimeSpan.FromTicks(ticks);
                    }
                    var type = Type.GetType(entryDto.TypeFullName);
                    if (type is null)
                        return Task.FromResult(Outcome<ITimeLimitedRepositoryEntry>.Fail($"Could not obtain cached value type with name '{entryDto.TypeFullName}'"));

                    var value = JsonSerializer.Deserialize(entryDto.Value, type)!;
                    var entry = new SimpleCacheEntry(repositories, entryDto.Path, value, spawnTimeUtc, customLifeSpan, customMaxLifeSpan);
                    return Task.FromResult(Outcome<ITimeLimitedRepositoryEntry>.Success(entry));
                }
                catch (Exception ex)
                {
                    return Task.FromResult(Outcome<ITimeLimitedRepositoryEntry>.Fail(ex));
                }
            }
            
            public EntryDto(string path, string spawnTimeUtc, string value, string typeFullName)
            {
                Path = path;
                SpawnTimeUtc = spawnTimeUtc;
                Value = value;
                TypeFullName = typeFullName;
            }
        }
    }
}