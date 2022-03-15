using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.Auth;
using TetraPak.XP.Caching;
using TetraPak.XP.Logging;

namespace authClient.console
{
    class spike_WindowsSecureCache : SimpleCache, ITokenCache
    {
        public override bool IsTypeStrict
        {
            get => false; 
            set { /* ignore */}
        }

        public spike_WindowsSecureCache(IDataProtectionProvider protectionProvider,  ILog? log) 
        : base(log, new spike_WindowsSecureCacheDelegate(protectionProvider, log))
        {
        }
    }

    static class spike_WindowsTokenCacheHelper
    {
        public static IServiceCollection AddTokenCaching(this IServiceCollection collection)
        {
            collection.AddDataProtection();
            collection.UseTokenCache<spike_WindowsSecureCache>();
            return collection;
        }
    }
}