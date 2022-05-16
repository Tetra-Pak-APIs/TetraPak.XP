using System;
using System.Threading.Tasks;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Mobile;
using TetraPak.XP.Mobile.iOS;
using UIKit;

[assembly:XpService(typeof(IPlatformService), typeof(PlatformService))]

namespace TetraPak.XP.Mobile.iOS
{
    sealed class PlatformService : IPlatformService
    {
        async Task<Outcome> IPlatformService.TryCloseTopWindowAsync(bool isModalWindow, bool animated)
        {
            var window = UIApplication.SharedApplication.KeyWindow;
            var viewController = window?.RootViewController;
            if (viewController is null)
                return Outcome.Fail("Couldn't obtain top window view controller");

            try
            {
                if (isModalWindow)
                {
                    viewController.DismissModalViewController(animated);
                    return Outcome.Success();
                }
                await viewController.DismissViewControllerAsync(animated);
                return Outcome.Success();
            }
            catch (Exception ex)
            {
                return Outcome.Fail(ex);
            }
        }
    }
}