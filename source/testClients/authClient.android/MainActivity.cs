using Android.App;
using Android.Content.PM;
using Android.OS;
using SegmentedControl.FormsPlugin.Android;
using TetraPak.XP.Android;
using Xamarin.Forms.Platform.Android;
using Resource = authClient.android.Resource;

namespace authClient.Android
{
    [Activity(Label = "authClient.Xamarin", Theme = "@style/MainTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Xamarin.Forms.Forms.Init(this, savedInstanceState);
            SegmentedControlRenderer.Init();
            TetraPakMainActivity.OnCreate(this, savedInstanceState);
            LoadApplication(new App());
        }
    }
}