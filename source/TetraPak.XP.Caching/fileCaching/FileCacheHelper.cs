using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.Caching.Abstractions;
using TetraPak.XP.Logging.Abstractions;

namespace TetraPak.XP.Caching
{
    public static class FileCacheHelper
    {
        static readonly object s_syncRoot = new();
        static ITimeLimitedRepositories? s_cache;

        public static IServiceCollection AddFileCache(this IServiceCollection services, FileCacheOptions? options)
        {
            services.AddSingleton<IFileCache>(p =>
            {
                var fileSystem = p.GetRequiredService<IFileSystem>(); 
                var log = p.GetService<ILog>();
                options ??= FileCacheOptions.Default(fileSystem);
                var fileCache = new SimpleFileCache(log, fileSystem, options);
                fileCache.AddFileCacheSupport(fileSystem, log, options);
                return fileCache;
            });
            return services;
        }

        public static ITimeLimitedRepositories AddFileCacheSupport(
            this ITimeLimitedRepositories? cache,
            IFileSystem fileSystem,
            ILog? log,
            FileCacheOptions? options = null)
        {
            cache ??= ensureCache(cache, log);
            options ??= FileCacheOptions.Default(fileSystem);
            cache. AddDelegates(new FileCacheDelegate(CacheNames.FileCache, options, log));
            return cache;
        }
        
        static ITimeLimitedRepositories ensureCache(ITimeLimitedRepositories? cache, ILog? log)
        {
            lock (s_syncRoot)
            {
                cache ??= s_cache ?? new SimpleCache(log);
                s_cache ??= cache;
                return cache;
            }
        }
    }
}