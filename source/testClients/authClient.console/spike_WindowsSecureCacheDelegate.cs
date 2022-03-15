using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using TetraPak.XP;
using TetraPak.XP.Caching;
using TetraPak.XP.Caching.Abstractions;
using TetraPak.XP.DynamicEntities;
using TetraPak.XP.Logging;

namespace authClient.console
{
    class spike_WindowsSecureCacheDelegate : SimpleCacheDelegate
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
            var stringValue = value as string ?? (value as IStringValue)?.StringValue;
            if (stringValue is null)
                return Outcome.Fail("Not a string value");

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
            entry.UpdateValue(unprotected);
            return Outcome<ITimeLimitedRepositoryEntry>.Success(entry);
        }

        internal spike_WindowsSecureCacheDelegate(IDataProtectionProvider protector, ILog? log) : base(log)
        {
            _protector = protector.CreateProtector(nameof(spike_WindowsSecureCache));
        }
    }
}