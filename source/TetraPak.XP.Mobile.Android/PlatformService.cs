using System.Threading.Tasks;
using TetraPak.XP;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Mobile.Android;

[assembly:XpService(typeof(IPlatformService), typeof(PlatformService))]

namespace TetraPak.XP.Mobile.Android
{
    sealed class PlatformService : IPlatformService
    {
        public RuntimePlatform RuntimePlatform => RuntimePlatform.Android;

        Task<Outcome> IPlatformService.TryCloseTopWindowAsync(bool isModalWindow, bool animated)
        {
            // todo Implement IPlatformService.TryCloseTopWindowAsync for Android when/if this becomes possible
            return  Task.FromResult(Outcome.Fail("Not (yet) possible for Android"));
        }
    }
}