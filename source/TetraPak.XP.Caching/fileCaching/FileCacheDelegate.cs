using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TetraPak.XP.Caching.Abstractions;
using TetraPak.XP.DynamicEntities;
using TetraPak.XP.Logging.Abstractions;

namespace TetraPak.XP.Caching
{
    /// <summary>
    ///   Work in progress - not recommended for use - Jonas Rembratt, 2022-04-29
    /// </summary>
    sealed class FileCacheDelegate : IITimeLimitedRepositoriesDelegate // todo implement FileCacheDelegate
    {
        readonly string _targetRepository;
        readonly ILog? _log;
        readonly Dictionary<string, ITimeLimitedRepositoryEntry> _entries = new();
        readonly FileCacheOptions _options;
        readonly TaskCompletionSource<bool> _loadEntriesTcs;

        public TimeSpan AutoPurgeInterval { get; set; }

        DirectoryInfo Directory { get; }
        
        public Task<Outcome<CachedItem<T>>> GetValidItemAsync<T>(ITimeLimitedRepositoryEntry entry)
        {
            throw new NotImplementedException();
        }

        public Task<Outcome<ITimeLimitedRepositoryEntry>> ReadRawEntryAsync(DynamicPath path)
        {
            throw new NotImplementedException();
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
        
        TaskCompletionSource<bool> ensureDirectoryAndLoadEntriesFromFileAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            Task.Run(async () =>
            {
                var directoryOutcome = ensureDirectory();
                if (!directoryOutcome)
                {
                    // could not create cache directory ...
                    tcs.TrySetException(directoryOutcome.Exception!);
                    return;
                }

                var path = Path.Combine(Directory.FullName, $".entries{_options.FileSuffix}");
                var file = new FileInfo(path);
                if (!file.Exists)
                {
                    // no entries has been created yet ...
                    tcs.SetResult(true);
                    return;
                }

                using (var stream = file.OpenRead())
                {
                    try
                    {
                        var entries = await JsonSerializer.DeserializeAsync<IEnumerable<FileCacheEntry>>(stream);
                        var array = entries.ToArray();
                        for (var i = 0; i < array.Length; i++)
                        {
                            
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex, $"Failed (JSON error, see exception) when loading file cache entries from {file.FullName}");
                    }
                }
            });
            return tcs;

            Outcome ensureDirectory()
            {
                if (!Directory.Exists)
                   return Outcome.Success();

                try
                {
                    Directory.Create();
                    return Outcome.Success();
                }
                catch (Exception ex)
                {
                    _log.Error(ex, $"Failed to create file cache directory: {Directory}");
                    return Outcome.Fail(ex);
                }
            }
        }
        
        public FileCacheDelegate(string targetRepository, FileCacheOptions options, ILog? log)
        {
            _targetRepository = targetRepository;
            _options = options ?? throw new ArgumentNullException(nameof(options));
            Directory = options.Directory;
            _log = log;
            _loadEntriesTcs = ensureDirectoryAndLoadEntriesFromFileAsync();
        }
    }
}