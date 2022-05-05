using System;
using System.Threading.Tasks;
using mobileClient.iOS;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Mobile;
using UIKit;

[assembly:XpService(typeof(IPlatformService), typeof(PlatformService))]

namespace mobileClient.iOS
{
    public class PlatformService : IPlatformService
    {
        public async Task CloseTopWindowAsync(bool isModalWindow, bool animated = true)
        {
            var window = UIApplication.SharedApplication.KeyWindow;
            var viewController = window?.RootViewController;
            if (viewController is null)
                return;

            if (isModalWindow)
            {
                viewController.DismissModalViewController(animated);
                return;
            }
            await viewController.DismissViewControllerAsync(animated);
        }

        public PlatformService()
        {
            Console.WriteLine($"nisse - instantiating iOS {typeof(IPlatformService)}");
        }
    }
}