using System.Threading.Tasks;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Mobile;
using TetraPak.XP.Mobile.Android;

[assembly:XpService(typeof(IPlatformService), typeof(PlatformService))]

namespace TetraPak.XP.Mobile.Android
{
    sealed class PlatformService : IPlatformService
    {
        Task<Outcome> IPlatformService.TryCloseTopWindowAsync(bool isModalWindow, bool animated)
        {
            // todo Implement IPlatformService.TryCloseTopWindowAsync for Android
            return  Task.FromResult(Outcome.Fail("Not yet implemented for Android"));
            // var window = UIApplication.SharedApplication.KeyWindow;
            // var viewController = window?.RootViewController;
            // if (viewController is null)
            //     return Outcome.Fail("Couldn't obtain top window view controller");
            //
            // try
            // {
            //     if (isModalWindow)
            //     {
            //         viewController.DismissModalViewController(animated);
            //         return Outcome.Success();
            //     }
            //     await viewController.DismissViewControllerAsync(animated);
            //     return Outcome.Success();
            // }
            // catch (Exception ex)
            // {
            //     return Outcome.Fail(ex);
            // }
        }
    }
}