using Android.App;
using Android.Content;
using Android.OS;
using TetraPak.XP.Android;

namespace authClient.Android
{
    [Activity(Label = "TetraPakAuthActivity")]
    [IntentFilter(
        actions: new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataSchemes = new[] { "testping" },
        DataPath = "/auth"
    )]
    public class AuthActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            TetraPakAuthActivity.OnCreate(this, Intent);
        }
    }
}