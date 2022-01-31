using TetraPak.XP.Caching;

namespace TetraPak.XP.Xamarin.Caching
{
    public static class XamarinSecureStoreHelper
    {
        public static ITimeLimitedRepositories AddSecureStore(this ITimeLimitedRepositories timeLimitedRepositories)
        {
            timeLimitedRepositories.AddDelegates(new SecureStoreCacheDelegate());
            return timeLimitedRepositories;
        }
    }
}