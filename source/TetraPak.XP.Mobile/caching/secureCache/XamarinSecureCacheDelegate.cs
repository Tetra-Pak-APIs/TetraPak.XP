using System;
using System.Threading.Tasks;
using TetraPak.XP.Caching;
using TetraPak.XP.Caching.Abstractions;
using TetraPak.XP.DynamicEntities;
using TetraPak.XP.Logging;
using TetraPak.XP.Logging.Abstractions;
using Xamarin.Essentials;

namespace TetraPak.XP.Mobile
{
    sealed class XamarinSecureCacheDelegate : ISecureCacheDelegate
    {
        readonly string _targetRepository;
        readonly ILog? _log;

        public TimeSpan AutoPurgeInterval { get; set; }
        public Task<Outcome<CachedItem<T>>> GetValidItemAsync<T>(ITimeLimitedRepositoryEntry entry)
        {
            var path = (RepositoryPath)entry.Path;
            if (!isTargetedRepository(path.Repository))
            {
                var exception = new InvalidOperationException($"Unexpected repository when validating entry: {path.Repository}");
                _log.Error(exception, messageId:(string) null!);
                throw exception;
            }

            var cachedItem = new CachedItem<T>(entry.Path, (T) entry.GetValue(), entry.ExpiresUtc()); 
            throw new NotImplementedException();
        }

        public async Task<Outcome<ITimeLimitedRepositoryEntry>> ReadRawEntryAsync(DynamicPath path)
        {
            var json = await SecureStorage.GetAsync(path);
            if (string.IsNullOrEmpty(json))
                return Outcome<ITimeLimitedRepositoryEntry>.Fail(
                    new ArgumentOutOfRangeException(nameof(path), $"Cached value not fund: {path}"));
                
            var entry = System.Text.Json.JsonSerializer.Deserialize<SimpleCacheEntry>(json);
            return Outcome<ITimeLimitedRepositoryEntry>.Success(entry);
        }

        public Task<Outcome> UpdateAsync(ITimeLimitedRepositoryEntry entry, bool strict)
        {
            throw new NotImplementedException();
        }

        public Task<Outcome> DeleteAsync(DynamicPath path, bool strict)
        {
            throw new NotImplementedException();
        }

        public Task<Outcome> CreateAsync(ITimeLimitedRepositoryEntry entry, bool strict)
        {
            throw new NotImplementedException();
        }

        public Task PurgeNowAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Outcome> CreateOrUpdateAsync(ITimeLimitedRepositoryEntry entry, bool strict)
        {
            throw new NotImplementedException();
        }

        bool isTargetedRepository(string repository) => repository == _targetRepository;

        public XamarinSecureCacheDelegate(string targetRepository, ILog? log)
        {
            _targetRepository = targetRepository;
            _log = log;
        }
    }
}