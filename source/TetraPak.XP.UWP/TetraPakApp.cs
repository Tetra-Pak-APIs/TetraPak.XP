using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.System;
using TetraPak.XP.Auth;
using TetraPak.XP.DependencyInjection;
using Xamarin.Forms;
using UWPApplication=Windows.UI.Xaml.Application;

namespace TetraPak.XP.UWP
{
    public class TetraPakAppDelegate : IAuthorizingAppDelegate
    {
        UWPApplication _app;

        internal static TetraPakAppDelegate Current { get; set; }

        public static void Init(UWPApplication app)
        {
            Current = new TetraPakAppDelegate(app);
        }

        public void ActivateApp()
        {
            throw new NotImplementedException();
        }

        public async Task OpenInDefaultBrowserAsync(Uri uri)
        {
            await ((IAuthorizingAppDelegate) this).OpenInDefaultBrowserAsync(uri, null);
        }

        public async Task OpenInDefaultBrowserAsync(Uri uri, Uri redirectUri)
        {
            await Launcher.LaunchUriAsync(uri);
        }

        TetraPakAppDelegate(UWPApplication app)
        {
            _app = app;
            Authorization.RegisterAppDelegate(() => this);
        }

        public static void OnActivated(IActivatedEventArgs args)
        {
            if (!(args is ProtocolActivatedEventArgs protocolActivatedEvent))
                return;

            // todo Ensure it's an expected uri
            var authCallbackHandler = XpServices.Get<IAuthCallbackHandler>();
            authCallbackHandler?.HandleUrlCallback(protocolActivatedEvent.Uri);
        }
    }

    public class TetraPakApp : UWPApplication, IAuthorizingAppDelegate
    {
        Uri _redirectUri;

        async void IAuthorizingAppDelegate.ActivateApp()
        {
            TetraPakAppDelegate.Current.ActivateApp();
        }

        async Task IAuthorizingAppDelegate.OpenInDefaultBrowserAsync(Uri uri)
            => await TetraPakAppDelegate.Current.OpenInDefaultBrowserAsync(uri, null);

        async Task IAuthorizingAppDelegate.OpenInDefaultBrowserAsync(Uri uri, Uri redirectUri)
            => await TetraPakAppDelegate.Current.OpenInDefaultBrowserAsync(uri, redirectUri);

        protected override void OnActivated(IActivatedEventArgs args)
        {
            TetraPakAppDelegate.OnActivated(args);
            base.OnActivated(args);
        }

        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);
        }

        public TetraPakApp() 
        {
            TetraPakAppDelegate.Init(this);
        }
    }
}