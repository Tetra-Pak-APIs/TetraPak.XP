using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.Caching;
using TetraPak.XP.Logging;
using TetraPak.XP.Logging.Abstractions;

namespace TetraPak.XP.Auth
{
    public sealed class DiscoveryDocumentCache : SimpleFileCache
    {
        internal const string DiscoFileSuffix = ".taxDiscoDocument";

        static FileCacheOptions ensureDiscoOptions(FileCacheOptions? options, IFileSystem fileSystem)
            => options?.WithFileSuffix(DiscoFileSuffix) ?? FileCacheOptions.Default(fileSystem, DiscoFileSuffix);
        
        public DiscoveryDocumentCache(IFileSystem fileSystem, ILog? log, FileCacheOptions? options)
        : base(log, fileSystem, ensureDiscoOptions(options, fileSystem))
        {
        }
            
        public DiscoveryDocumentCache(SimpleFileCache implementation, ILog? log, IFileSystem fileSystem, FileCacheOptions? options)
        : base(implementation, log, fileSystem, ensureDiscoOptions(options, fileSystem))
        {
        }
    }

    public static class OidcHelper
    {
        public static IServiceCollection AddDiscoveryDocumentCache(this IServiceCollection services)
        {
            services.AddSingleton(p =>
            {
                var fileSystem = p.GetRequiredService<IFileSystem>(); 
                var log = p.GetService<ILog>();
                var options = FileCacheOptions.Default(fileSystem, DiscoveryDocumentCache.DiscoFileSuffix);
                var implementation = p.GetService<IFileCache>() as SimpleFileCache;
                return new DiscoveryDocumentCache(implementation!, log, fileSystem, options);
            });
            return services;
        }
    }
    
}