using System;
using System.Threading.Tasks;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Mobile;
using TetraPak.XP.Mobile.iOS;
using UIKit;

[assembly:XpService(typeof(IPlatformService), typeof(IosPlatformService))]

namespace TetraPak.XP.Mobile.iOS
{
    public sealed class IosPlatformService : IPlatformService
    {
        public async Task<Outcome> TryCloseTopWindowAsync(bool isModalWindow, bool animated = true)
        {
            var window = UIApplication.SharedApplication.KeyWindow;
            var viewController = window.RootViewController;
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

        public IosPlatformService()
        {
            Console.WriteLine($"nisse - instantiating iOS {typeof(IPlatformService)}"); // nisse
        }
    }
}