using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TetraPak.XP.Browsers;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Desktop;
using TetraPak.XP.Logging.Abstractions;
using TetraPak.XP.Web.Abstractions;
using TetraPak.XP.Web.Http;

[assembly:XpService(typeof(ILoopbackBrowser), typeof(DesktopLoopbackBrowser))]

namespace TetraPak.XP.Desktop
{
    public sealed class DesktopLoopbackBrowser : LoopbackBrowser
    {
        protected override Task OnOpenBrowserAsync(Uri uri) // todo redesign to support opening browsers on all (also mobile) platforms
        {
            Log.Trace(uri.ToStringBuilderAsync(ToString(), null).Result.ToString());
            var url = uri.AbsoluteUri;
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }

            return Task.CompletedTask;
        }

        public DesktopLoopbackBrowser(ILog? log = null) 
        : base(log)
        {
        }
    }
}