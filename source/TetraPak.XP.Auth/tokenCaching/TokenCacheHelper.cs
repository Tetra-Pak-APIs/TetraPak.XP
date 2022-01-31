using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.Caching;

namespace TetraPak.XP.Auth
{
    public static class TokenCacheHelper
    {
        internal const string TokenCacheRepository = "SecurityTokens";
        
        public static IServiceCollection AddTokenCache(this IServiceCollection services)
        {
            services.AddSingleton<ITokenCache>(p =>
            {
                var secureCache = p.GetRequiredService<ISecureCache>();
                AddTokenCacheSupport(secureCache);
                
            });

        }

        public static ITimeLimitedRepositories AddTokenCacheSupport(this ITimeLimitedRepositories cache)
        {
        }
        
    }
}