// using System;
// using System.Threading.Tasks;
// using mobileClient.iOS;
// using TetraPak.XP;
// using TetraPak.XP.DependencyInjection;
// using TetraPak.XP.Mobile;
// using UIKit;
//
// [assembly:XpService(typeof(IPlatformService), typeof(IosPlatformService))] obsolete
//
// namespace mobileClient.iOS
// {
//     public sealed class IosPlatformService : IPlatformService
//     {
//         public async Task<Outcome> TryCloseTopWindowAsync(bool isModalWindow, bool animated = true)
//         {
//             var window = UIApplication.SharedApplication.KeyWindow;
//             var viewController = window.RootViewController;
//             if (viewController is null)
//                 return Outcome.Fail("Couldn't obtain top window view controller");
//
//             try
//             {
//                 if (isModalWindow)
//                 {
//                     viewController.DismissModalViewController(animated);
//                     return Outcome.Success();
//                 }
//                 await viewController.DismissViewControllerAsync(animated);
//                 return Outcome.Success();
//             }
//             catch (Exception ex)
//             {
//                 return Outcome.Fail(ex);
//             }
//         }
//
//         public IosPlatformService()
//         {
//             Console.WriteLine($"nisse - instantiating iOS {typeof(IPlatformService)}");
//         }
//     }
// }