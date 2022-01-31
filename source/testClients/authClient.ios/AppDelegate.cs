using Foundation;
using SegmentedControl.FormsPlugin.iOS;
using TetraPak.Auth.Xamarin.iOS;
using UIKit;
using Xamarin.Forms.Platform.iOS;

namespace authClient.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Xamarin.Forms.Forms.Init();
            SegmentedControlRenderer.Init();
            LoadApplication(new App());
            TetraPakAppDelegate.OnFinishedLaunching(app, options);
            return base.FinishedLaunching(app, options);
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            return TetraPakAppDelegate.OnOpenUrl(app, url, options);
        }
    }
}