using System;
using System.Threading.Tasks;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Browsers;
using TetraPak.XP.Configuration;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Logging.Abstractions;
using TetraPak.XP.Mobile;
using TetraPak.XP.Web.Abstractions;
using TetraPak.XP.Web.Http;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly:XpService(typeof(ILoopbackBrowser), typeof(MobileLoopbackBrowser))]

namespace TetraPak.XP.Mobile
{
    public sealed class MobileLoopbackBrowser : LoopbackBrowser
    {
        readonly ITetraPakConfiguration _tetraPakConfig;
        readonly BrowserExperience _browserExperience;

        protected override async Task OnOpenBrowserAsync(Uri uri)
        {
            Application.Current.PageAppearing += onPageAppearing;
            
            Log.Trace(uri.ToStringBuilderAsync(ToString(), null).Result.ToString());
            // todo make choice of internal/external browser configurable 
            await Browser.OpenAsync(uri, new BrowserLaunchOptions
            {
                LaunchMode = BrowserLaunchMode.SystemPreferred
            });
        }

        protected override async Task<Outcome<GenericHttpRequest>> OnOutcomeAsync(Outcome<GenericHttpRequest> outcome)
        {
            var platformService = XpServices.Get<IPlatformService>();
            if (platformService is { })
            {
                await platformService.TryCloseTopWindowAsync(true);
            }
            return await base.OnOutcomeAsync(outcome);
        }

        void onPageAppearing(object sender, Page e)
        {
            Application.Current.PageAppearing -= onPageAppearing;
            DisposeLoopbackHost();
        }

        public MobileLoopbackBrowser(
            ITetraPakConfiguration tetraPakConfig,
            ILog? log = null) 
        : base(log)
        {
            _tetraPakConfig = tetraPakConfig;
            _browserExperience = tetraPakConfig.GetNamed<BrowserExperience>(nameof(AuthConfiguration.BrowserExperience));
        }

        public MobileLoopbackBrowser(BrowserExperience browserExperience, ILog? log)
        : base(log)
        {
            _tetraPakConfig = null!;
            _browserExperience = browserExperience;
        }
    }
}