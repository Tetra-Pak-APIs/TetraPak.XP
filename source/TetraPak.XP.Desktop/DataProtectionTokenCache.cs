using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.Auth;
using TetraPak.XP.Caching;
using TetraPak.XP.Logging.Abstractions;

namespace TetraPak.XP.Desktop
{
    class DataProtectionTokenCache : SimpleCache, ITokenCache
    {
        public override bool IsTypeStrict
        {
            get => false; 
            set { /* ignore */ }
        }

        public DataProtectionTokenCache(IDataProtectionProvider protectionProvider,  ILog? log = null) 
        : base(log, new DataProtectionSecureCacheDelegate(protectionProvider, log))
        {
        }
    }

    public static class DataProtectionTokenCacheHelper
    {
        public static IServiceCollection AddDesktopTokenCache(this IServiceCollection collection)
        {
            collection.AddDataProtection();
            collection.UseTokenCache<DataProtectionTokenCache>();
            return collection;
        }
    }
}