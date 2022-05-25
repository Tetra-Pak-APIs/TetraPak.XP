using System;
using System.Threading;
using System.Threading.Tasks;
using TetraPak.XP;
using TetraPak.XP.Logging.Abstractions;
using TetraPak.XP.Web.Abstractions;
using Xamarin.Forms;

namespace mobileClient.ViewModels
{
    /// <summary>
    ///   This view model can be used as a loopback browser, and is a good example for how
    ///   you can 
    /// </summary>
    public sealed class LoopbackBrowserVM : BaseViewModel, ILoopbackBrowser
    {
        readonly ILog? _log;
        public Task<string> HtmlResponseOnSuccessFactory { get; set; }
        public Task<string> HtmlResponseOnErrorFactory { get; set; }

        public async Task<Outcome<GenericHttpRequest>> GetLoopbackAsync(
            Uri target, 
            Uri loopbackHost, 
            LoopbackFilter? filter = null,
            CancellationTokenSource? cancellationTokenSource = null, 
            TimeSpan? timeout = null)
        {
            var tcs = new TaskCompletionSource<Outcome<GenericHttpRequest>>();
            var webView = new WebView
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                Source = new UrlWebViewSource { Url = target.AbsoluteUri }
            };
            webView.Navigating += onWebNavigating;
            var page = new ContentPage
            {
                Content = webView,
            };
            page.Disappearing += onPageDisappearing;

            await PushAsync(page);
            var isPageClosed = false;
            var outcome = await tcs.GetOutcomeAsync(cancellationTokenSource, timeout);
            if (isPageClosed)
                return outcome;
                
            await PopAsync();
            return outcome;
            
            void onWebNavigating(object sender, WebNavigatingEventArgs e)    
            {
                cancellationTokenSource?.Token.ThrowIfCancellationRequested();
                _log.Trace($">>> {e.Url}");

                var uri = new Uri(e.Url);
                
                if (!uri.Authority.Equals(loopbackHost.Authority, StringComparison.InvariantCultureIgnoreCase))
                    return;

                var request = new GenericHttpRequest { Uri = new Uri(e.Url) };
                tcs.SetResult(Outcome<GenericHttpRequest>.Success(request));
            }
            
            void onPageDisappearing(object sender, EventArgs e)
            {
                if (tcs.Task.IsCompleted)
                    return;

                isPageClosed = true;
                tcs.SetCanceled();
            }
        }

        public LoopbackBrowserVM(ILog? log)
        {
            HtmlResponseOnErrorFactory = null!;
            HtmlResponseOnSuccessFactory = null!;
            _log = log;
        }

        public void Dispose()
        {
            // ignore
        }

    }
}