using System;
using System.Threading.Tasks;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Browsers;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Logging.Abstractions;
using TetraPak.XP.Mobile;
using TetraPak.XP.Web.Abstractions;
using TetraPak.XP.Web.Http;
using Xamarin.Essentials;

[assembly:XpService(typeof(ILoopbackBrowser), typeof(MobileLoopbackBrowser))]

namespace TetraPak.XP.Mobile
{
    public class MobileLoopbackBrowser : LoopbackBrowser
    {
        readonly ITetraPakConfiguration _tetraPakConfig;

        protected override async Task OnOpenBrowserAsync(Uri uri)
        {
            Log.Trace(uri.ToStringBuilderAsync(ToString(), null).Result.ToString());
            // todo make choice of internal/external browser configurable 
            await Browser.OpenAsync(uri, new BrowserLaunchOptions
            {
                LaunchMode = BrowserLaunchMode.SystemPreferred,
            } );
        }
        
        public MobileLoopbackBrowser(ITetraPakConfiguration tetraPakConfig, ILog? log = null) 
        : base(log)
        {
            _tetraPakConfig = tetraPakConfig;
        }
    }
}