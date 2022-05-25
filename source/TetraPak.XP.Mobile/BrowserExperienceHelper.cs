using System;
using Xamarin.Essentials;

namespace TetraPak.XP.Mobile
{
    static class BrowserExperienceHelper
    {
        public static BrowserLaunchMode ToBrowserLaunchMode(this BrowserExperience browserExperience)
        {
            return browserExperience switch
            {
                BrowserExperience.ExternalSystem => BrowserLaunchMode.External,
                BrowserExperience.InternalSystem => BrowserLaunchMode.SystemPreferred,
                _ => throw new ArgumentOutOfRangeException(nameof(browserExperience))
            };
        }
    }
}