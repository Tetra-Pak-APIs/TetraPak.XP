using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Web.Http;

namespace TetraPak.XP.OAuth2.Refresh
{
    public static class RefreshTokenServiceHelper
    {
        const string GrantCacheRepoKey = "__grantCacheRepository";

        static bool s_isRefreshTokenAdded;
        static readonly object s_syncRoot = new();
        
        /// <summary>
        ///   (fluent api)<br/>
        ///   Adds support for OAuth Authorization Code grant to the application and returns the <paramref name="collection"/>. 
        /// </summary>
        public static IServiceCollection UseTetraPakRefreshTokenGrant(this IServiceCollection collection)
        {
            lock (s_syncRoot)
            {
                if (s_isRefreshTokenAdded)
                    return collection;

                s_isRefreshTokenAdded = true;
            }
            
            collection.AddTetraPakConfiguration();
            collection.UseTetraPakHttpClientProvider();
            collection.AddSingleton<IRefreshTokenGrantService,TetraPakRefreshTokenGrantService>();
            return collection;
        }

        // public static GrantOptions WithGrantCacheRepository(this GrantOptions options, string grantCacheRepository) obsolete
        // {
        //     return options.WithData(GrantCacheRepoKey, grantCacheRepository);
        // }
        //
        // public static string GetGrantCacheRepository(this GrantOptions options)
        // {
        //     return options.GetData<string>(GrantCacheRepoKey);
        // }
    }
}