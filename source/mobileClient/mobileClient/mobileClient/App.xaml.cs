using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using mobileClient.Services;
using mobileClient.ViewModels;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Identity;
using TetraPak.XP.Logging;
using TetraPak.XP.Mobile;
using TetraPak.XP.OAuth2;
using TetraPak.XP.OAuth2.ClientCredentials;
using TetraPak.XP.OAuth2.DeviceCode;
using TetraPak.XP.OAuth2.OIDC;
using TetraPak.XP.OAuth2.TokenExchange;
using TetraPak.XP.Web.Services;
using Xamarin.Forms;

namespace mobileClient
{
    public sealed partial class App : Application
    {
        public INavigation Navigation => MainPage.Navigation;
        
        public App()
        {
            InitializeComponent();
            // spike
            this.BuildTetraPakMobileHost(collection =>
            {
                collection
                    .AddTetraPakWebServices()
                    .AddMobileTokenCache()
                    .AddAppCredentialsDelegate<CustomAppCredentialsDelegate>()
                    .AddTetraPakOidcGrant()
                    .AddTetraPakClientCredentialsGrant()
                    .AddTetraPakDeviceCodeGrant()
                    .AddTetraPakTokenExchangeGrant()
                    .AddTetraPakUserInformation()
                    .AddViewModels()
                    // just a very basic log (abstracted by the ILog interface, you can use something else here, like NLog, SemiLog, Log4Net or whatever) obsolete
                    .AddSingleton(p => new LogBase(p.GetService<IConfiguration>()).WithConsoleLogging());
                    //.AddMicrosoftLogging(new LogFormatOptions { SuppressRank = true, SuppressPrefix = true});
            }).ServiceCollection.BuildXpServiceProvider();
            
            DependencyService.Register<MockDataStore>(); // obsolete
            MainPage = new AppShell();
            
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
