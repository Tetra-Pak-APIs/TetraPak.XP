using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TetraPak.XP.Browsers;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Desktop;
using TetraPak.XP.Logging;

[assembly:XpService(typeof(DesktopLoopbackBrowser))]

namespace TetraPak.XP.Desktop
{
    public class DesktopLoopbackBrowser : LoopbackBrowser
    {
        protected override void OnOpenBrowser(Uri uri) // todo redesign to support opening browsers on all (also mobile) platforms
        {
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
        }

        public DesktopLoopbackBrowser(ILog? log = null) 
        : base(log)
        {
        }
    }
}