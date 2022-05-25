using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using mobileClient.ViewModels;
using TetraPak.XP;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Logging;
using TetraPak.XP.Logging.Abstractions;
using TetraPak.XP.Mobile;
using TetraPak.XP.OAuth2;
using TetraPak.XP.Web.Abstractions;
using TetraPak.XP.Web.Services;
using Xamarin.Forms;

[assembly:ExportFont("FontAwesome6Brands.otf", Alias = "FontAwesome6Brands")]
[assembly:ExportFont("FontAwesome6Regular.otf", Alias = "FontAwesome6Regular")]
[assembly:ExportFont("FontAwesome6Solid.otf", Alias = "FontAwesome6Solid")]

namespace mobileClient
{
    public sealed partial class App
    {
        public INavigation Navigation => MainPage.Navigation;
        
        public App()
        {
            InitializeComponent();
            this.BuildTetraPakMobileHost(collection =>
            {
                var logOptions = LogFormatOptions.Default.WithOmitTimestamp(true);
                collection
                    .AddSingleton(p =>
                    {
                        /*
                           For Android we're using an internal web view for loopback web requests. 
                           the reason for this is that on Android, unlike iOS, there is no way to automatically close 
                           the system browser after auth is completed on Android. Also, Tetra Pak does
                           not manage Android devices (at this time -May/2022) so there is no need to 
                           support certificate challenges where the browser needs access to the device cert.
                        */
                        var platform = p.GetRequiredService<IPlatformService>().RuntimePlatform;
                        return platform == RuntimePlatform.Android 
                            ? new LoopbackBrowserVM(p.GetService<ILog>()) 
                            : p.GetRequiredService<ILoopbackBrowser>();
                    })
                    .AddTetraPakMobileAuthorization(GrantType.OIDC, GrantType.DeviceCode)
                    .AddAppCredentialsDelegate<CustomAppCredentialsDelegate>()
                    .AddTetraPakWebServices()
                    .AddViewModels()
                    // just a very basic log (abstracted by the ILog interface, you can abstract and use something else here, like NLog, SemiLog, Log4Net or whatever)
                    .AddSingleton(p => 
                        new LogBase(p.GetService<IConfiguration>()).WithConsoleLogging(logOptions));
            }).ServiceCollection.BuildXpServiceProvider();
            MainPage = new AppShell();
            
        }
    }
}
