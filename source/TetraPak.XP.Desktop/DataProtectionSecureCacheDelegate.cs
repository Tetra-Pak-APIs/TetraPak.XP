using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using TetraPak.XP.Caching;
using TetraPak.XP.Caching.Abstractions;
using TetraPak.XP.DynamicEntities;
using TetraPak.XP.Logging.Abstractions;
using TetraPak.XP.StringValues;

namespace TetraPak.XP.Desktop
{
    sealed class DataProtectionSecureCacheDelegate : SimpleCacheDelegate
    {
        readonly IDataProtector _protector;

        public override async Task<Outcome> CreateAsync(ITimeLimitedRepositoryEntry entry, bool strict)
        {
            var protectOutcome = protect(entry);
            if (!protectOutcome)
                return protectOutcome;

            return await base.CreateAsync(entry, strict);
        }

        public override async Task<Outcome> UpdateAsync(ITimeLimitedRepositoryEntry entry, bool strict)
        {
            var protectOutcome = protect(entry);
            if (!protectOutcome)
                return protectOutcome;

            return await base.UpdateAsync(entry, strict);
        }

        public override async Task<Outcome> CreateOrUpdateAsync(ITimeLimitedRepositoryEntry entry, bool strict)
        {
            var protectOutcome = protect(entry);
            if (!protectOutcome)
                return protectOutcome;

            return await base.CreateOrUpdateAsync(entry, strict);
        }

        public override Task<Outcome<ITimeLimitedRepositoryEntry>> ReadRawEntryAsync(DynamicPath path)
        {
            return unprotect(path);
        }

        Outcome protect(ITimeLimitedRepositoryEntry entry)
        {
            var value = entry.GetValue();
            var stringValue = (value as string ?? (value as IStringValue)?.StringValue) ?? JsonSerializer.Serialize(value);
            entry.UpdateValue(_protector.Protect(stringValue));
            return Outcome.Success();
        }

        async Task<Outcome<ITimeLimitedRepositoryEntry>> unprotect(DynamicPath path)
        {
            var entryOutcome = await base.ReadRawEntryAsync(path);
            if (!entryOutcome)
                return entryOutcome;

            var entry = entryOutcome.Value!;
            if (entry.GetValue() is not string stringValue)
                throw new Exception("Not a string value");

            var unprotected = _protector.Unprotect(stringValue);
            var outEntry = entry.Clone();
            outEntry.UpdateValue(unprotected);
            return Outcome<ITimeLimitedRepositoryEntry>.Success(outEntry);
        }

        public override async Task<Outcome<CachedItem<T>>> GetValidItemAsync<T>(ITimeLimitedRepositoryEntry entry)
        {
            var value = entry.GetValue();
            if (value is T)
                return await base.GetValidItemAsync<T>(entry);

            if (value is not string stringValue)
                return Outcome<CachedItem<T>>.Fail($"Could not deserialize cached value '{entry.Path}'");

            if (typeof(T) == typeof(string) || typeof(T).IsImplementingInterface<IStringValue>())
                return await base.GetValidItemAsync<T>(entry);
                
            try
            {
                using var stream = stringValue.ToStream();
                var tValue = (await JsonSerializer.DeserializeAsync<T>(stream) ?? default(T))!;
                entry.UpdateValue(tValue);
                return await base.GetValidItemAsync<T>(entry);
            }
            catch (Exception ex)
            {
                ex = new Exception($"Could not deserialize cached value '{entry.Path}' (see inner)", ex);
                return Outcome<CachedItem<T>>.Fail(ex);
            }
        }

        internal DataProtectionSecureCacheDelegate(IDataProtectionProvider protector, ILog? log = null) 
        : base(log)
        {
            _protector = protector.CreateProtector(nameof(DataProtectionTokenCache));
        }
    }
}