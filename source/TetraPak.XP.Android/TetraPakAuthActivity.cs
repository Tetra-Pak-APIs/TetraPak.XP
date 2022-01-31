using System;
using Android.App;
using Android.Content;
using Android.OS;
using TetraPak.XP.Android;
using TetraPak.XP.Auth;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Logging;
using Xamarin.Forms;
using ILog=TetraPak.XP.Logging.ILog;

[assembly: XpService(typeof(TetraPakAuthActivity))]

namespace TetraPak.XP.Android
{
    // [Activity(Label = "TetraPakAuthActivity")] obsolete
    // [IntentFilter(
    //         actions: new[] { Intent.ActionView },
    //         Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    //         DataSchemes = new[] { "testping" },
    //         DataPath = "/auth"
    //         )]
    public class TetraPakAuthActivity : Activity
    {
        public static void OnCreate(Activity activity, Intent intent, bool finish = true)
        {
            var log = XpServices.Get<ILog>(); 
            global::Android.Net.Uri callbackUri = intent.Data;
            if (callbackUri?.ToString() is null)
            {
                log.Warning("Redirect was invoked with no data (exits)");
                Authorization.GetAuthorizingAppDelegate()!.ActivateApp();
                return;                    
            }
            var uri = new Uri(callbackUri.ToString()!);
            log.Debug($"OnCreate: Uri={uri}");
            Authorization.GetAuthorizingAppDelegate()!.ActivateApp();
            var authCallbackHandler = XpServices.Get<IAuthCallbackHandler>();
            authCallbackHandler?.HandleUrlCallback(uri);
            if (finish)
            {
                activity.Finish();
            }
        }
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            global::Android.Net.Uri androidUri = Intent.Data;
            var uri = new Uri(androidUri.ToString());
            var log = XpServices.Get<ILog>();
            log.Debug($"OnCreate: Uri={uri}");
            Authorization.GetAuthorizingAppDelegate()!.ActivateApp();
            var authCallbackHandler = XpServices.Get<IAuthCallbackHandler>();
            authCallbackHandler?.HandleUrlCallback(uri);
            Finish();
        }
    }
}
