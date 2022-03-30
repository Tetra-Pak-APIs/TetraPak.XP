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
            Directory = new DirectoryInfo(options.Directory);
            _log = log;
            _loadEntriesTcs = ensureDirectoryAndLoadEntriesFromFileAsync();
        }
    }

    public class FileCacheOptions
    {
        public const string DefaultFileSuffix = ".cache";
        
        public string Directory { get; }

        public string FileSuffix { get; private set; }

        public bool RetainInMemory { get; }

        public static FileCacheOptions Default(IFileSystem fileSystem, string? fileSuffix = null) =>
            new(fileSystem.GetCacheDirectory(), string.IsNullOrWhiteSpace(fileSuffix) ? DefaultFileSuffix : fileSuffix);

        public FileCacheOptions WithFileSuffix(string fileSuffix)
        {
            FileSuffix = fileSuffix;
            return this;
        }

        public FileCacheOptions(string directory, string? fileSuffix = null, bool retainInMemory = false)
        {
            Directory = string.IsNullOrWhiteSpace(directory) 
                ? throw new ArgumentNullException(nameof(directory)) 
                : directory;
            FileSuffix = string.IsNullOrWhiteSpace(fileSuffix) 
                ? DefaultFileSuffix 
                : fileSuffix!.EnsurePrefix('.');
            RetainInMemory = retainInMemory;
        }
    }

    [Serializable]
    class FileCacheEntry : ITimeLimitedRepositoryEntry
    {
        // public string CachePath { get; set; }
        
        public string FilePath { get; set; }

        public string Path { get; set;  }
        public DateTime SpawnTimeUtc { get; set; }
        public TimeSpan GetRemainingTime(DateTime? @from = null)
        {
            throw new NotImplementedException();
        }

        public Type GetValueType()
        {
            throw new NotImplementedException();
        }

        public object GetValue()
        {
            throw new NotImplementedException();
        }

        public void UpdateValue(object value, DateTime? spawnTimeUtc = null, TimeSpan? customLifeSpan = null)
        {
            throw new NotImplementedException();
        }

        public void ExtendLifeSpan(DateTime? spawnTimeUtc = null)
        {
            throw new NotImplementedException();
        }

        public ITimeLimitedRepositories Repositories { get; private set; }
        
        public ITimeLimitedRepositoryEntry Clone()
        {
            return new FileCacheEntry(Path, FilePath)
            {
                Repositories = Repositories,
                SpawnTimeUtc = SpawnTimeUtc
            };
        }

        public TimeSpan? CustomLifeSpan { get; set; }

        public TimeSpan? CustomMaxLifeSpan { get; set; }

        public DateTime? LastAccessedUtc { get; set; }

        public FileCacheEntry(string cachePath, string filePath)
        {
            Path = cachePath;
            FilePath = filePath;
        }
    }
    
namespace TetraPak.XP.Caching
{
    /// <summary>
    ///   This implementation of the <see cref="ICache{T}"/> interface relies on files saved within
    ///   a specified folder of the file system.
    /// </summary>
    
    
    static class TaskHelper
    {
        public static bool IsActive(this Task self)
        {
            return self.Status < TaskStatus.RanToCompletion;
        }
    }

}
}