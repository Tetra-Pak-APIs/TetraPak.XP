using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using TetraPak.XP.Android;
using TetraPak.XP.Auth;
using TetraPak.XP.DependencyInjection;
using Xamarin.Forms.Platform.Android;

[assembly: XpService(typeof(TetraPakMainActivity))]

namespace TetraPak.XP.Android
{
    public class TetraPakMainActivity : FormsAppCompatActivity, IAuthorizingAppDelegate
    {
        readonly FormsAppCompatActivity _appActivity;
        readonly Bundle _bundle;

        internal List<Uri> ExpectedRedirectUris { get; } = new List<Uri>();

        FormsAppCompatActivity AppActivity => _appActivity ?? this;

        internal static TetraPakMainActivity Current { get; private set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            if (_appActivity is null)
            {
                // was called by derived class ...
                base.OnCreate(savedInstanceState);
            }

            if (!Authorization.IsAppDelegateRegistered)
            {
                Authorization.RegisterAppDelegate(() => this);
            }
            Current = this;
        }

        /// <summary>
        ///   Initializes the Android, enabling authorization. 
        /// </summary>
        /// <param name="appActivity">
        ///   The calling (main) Activity.
        /// </param>
        /// <param name="savedInstanceState">
        ///   The Main Activity "context".
        /// </param>
        public static void OnCreate(FormsAppCompatActivity appActivity, Bundle savedInstanceState)
        {
            Current = new TetraPakMainActivity(appActivity, savedInstanceState);
            Current.OnCreate(savedInstanceState);
        }

        void IAuthorizingAppDelegate.ActivateApp()
        {
            var mainActivityType = AppActivity.GetType();
            var intent = new Intent(AppActivity, mainActivityType).SetFlags(ActivityFlags.ReorderToFront);
            AppActivity.StartActivity(intent);
        }

        Task IAuthorizingAppDelegate.OpenInDefaultBrowserAsync(Uri uri)
            => ((IAuthorizingAppDelegate)this).OpenInDefaultBrowserAsync(uri, null!);

        // todo Consider scrapping Android browser implementation and just use Xamarin.Essentials `Browser` api (with BrowserLaunchMode.SystemPreferred) instead   
        Task IAuthorizingAppDelegate.OpenInDefaultBrowserAsync(Uri uri, Uri redirectUri)
        {
            if (redirectUri != null)
            {
                ExpectedRedirectUris.Add(redirectUri);
            }

            var androidUri = global::Android.Net.Uri.Parse(uri.ToString());
            var browser = new Intent(Intent.ActionView, androidUri);
            browser.SetData(androidUri);
            var resolveInfo = AppActivity.PackageManager.ResolveActivity(browser, PackageInfoFlags.MatchDefaultOnly);
            var packageName = resolveInfo.ActivityInfo.PackageName;
            browser.SetPackage(packageName);
            try
            {
                AppActivity.StartActivity(browser);
            }
            catch (ActivityNotFoundException)
            {
                browser.SetPackage(null);
                AppActivity.StartActivity(Intent.CreateChooser(browser, "Select Browser"));
            }
            return Task.CompletedTask;
        }

        TetraPakMainActivity(FormsAppCompatActivity appActivity, Bundle savedInstanceState)
        {
            _appActivity = appActivity;
            _bundle = savedInstanceState;
        }
    }
}
