using System;
using Microsoft.Extensions.DependencyInjection;
using mobileClient.ViewModels;
using TetraPak.XP;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Logging.Abstractions;
using TetraPak.XP.Mobile;
using TetraPak.XP.Web.Abstractions;

namespace mobileClient
{
    public static class LoopbackBrowserHelper
    {
        public static IServiceCollection AddCustomLoopbackBrowser(this IServiceCollection collection)
        {
            /*
               For Android we're using an internal web view for loopback web requests. 
               The reason being on Android, unlike iOS, there is no way to force close 
               a system browser after auth is completed. 
               
               Also, Tetra Pak does not manage Android devices (at this time -May/2022) so there 
               is no need to support certificate challenges where the browser needs access to the 
               device cert so a system browser isn't needed anyway
            */

            collection.AddSingleton<ILoopbackBrowser>(p =>
            {
                var platform = p.GetRequiredService<IPlatformService>().RuntimePlatform;
                var log = p.GetService<ILog>();
                return platform == RuntimePlatform.Android 
                    ? new LoopbackBrowserVM(log) 
                    : new MobileLoopbackBrowser(
                        p.GetRequiredService<ITetraPakConfiguration>(), 
                        log);
            });
            return collection;
        }
    }
}