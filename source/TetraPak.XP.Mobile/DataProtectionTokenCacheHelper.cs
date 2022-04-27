using Microsoft.Extensions.DependencyInjection;

namespace TetraPak.XP.Mobile
{
    public static class DataProtectionTokenCacheHelper
    {
        public static IServiceCollection AddMobileTokenCache(this IServiceCollection collection)
        {
            // collection.AddDataProtection(); todo Support Xamarin secure store
            // collection.UseTokenCache<DataProtectionTokenCache>();
            return collection;
        }
    }
}