using System;
using System.Threading.Tasks;
using mobileClient.iOS;
using TetraPak.XP;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Mobile;
using UIKit;

[assembly:XpService(typeof(IPlatformService), typeof(PlatformService))]

namespace mobileClient.iOS
{
    public class PlatformService : IPlatformService
    {
        public async Task<Outcome> TryCloseTopWindowAsync(bool isModalWindow, bool animated = true)
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

        public PlatformService()
        {
            Console.WriteLine($"nisse - instantiating iOS {typeof(IPlatformService)}");
        }
    }
}