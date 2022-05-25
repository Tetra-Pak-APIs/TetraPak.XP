using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using TetraPak.XP.Logging.Abstractions;
using TetraPak.XP.Web.Abstractions;

namespace TetraPak.XP.Browsers
{
    /// <summary>
    ///   Implements a a basic abstract interactive browser. 
    /// </summary>
    public abstract class LoopbackBrowser : ILoopbackBrowser
    {
        const string DefaultHtmlResponseOnSuccess = "<h2>You can now return to the application.</h2>";
        const string DefaultHtmlResponseOnError = "<h2>Invalid request.</h2>";
        
        LoopbackHost? _loopbackHost;

        protected ILog? Log { get; }

        /// <summary>
        ///   HTML content to be sent to the loopback browser after the request/response roundtrip
        ///   completed without errors.
        /// </summary>
        public Task<string> HtmlResponseOnSuccessFactory { get; set; }

        /// <summary>
        ///   HTML content to be sent to the loopback browser after the request/response roundtrip
        ///   resulted in an exception.
        /// </summary>
        public Task<string> HtmlResponseOnErrorFactory { get; set; }

        Task<string> getDefaultHtmlResponseOnSuccess() => Task.FromResult(DefaultHtmlResponseOnSuccess);

        Task<string> getDefaultHtmlResponseOnError() => Task.FromResult(DefaultHtmlResponseOnError);

        public LoopbackFilter? LoopbackFilter { get; set; }

        public static Uri BuildLoopbackUri(string path, int port = -1, HttpScheme scheme = HttpScheme.Http)
        {
            var ub = new UriBuilder(scheme.ToString().ToLower(), IPAddress.Loopback.ToString());
            if (port > 0)
            {
                ub.Port = port;
            }
            ub.Path = path;
            return ub.Uri;
        }

        /// <inheritdoc />
        public Task<Outcome<GenericHttpRequest>> GetLoopbackAsync(
            Uri target, 
            Uri loopbackHost, 
            LoopbackFilter? filter = null,
            CancellationTokenSource? cancellationTokenSource = null, 
            TimeSpan? timeout = null)
        {
            try
            {
                this.WithLoopBackFilter(filter ?? DefaultLoopbackPatternFilter);
                var cancellationToken = cancellationTokenSource?.Token ?? CancellationToken.None;
                return invokeAsync(target, loopbackHost, cancellationToken, timeout);

            }
            catch (Exception ex)
            {
                return Task.FromResult(Outcome<GenericHttpRequest>.Fail(ex));
            }
        }
        
        async Task<Outcome<GenericHttpRequest>> invokeAsync(
            Uri targetUri,
            Uri loopbackHostUri,
            CancellationToken cancellationToken,
            TimeSpan? timeout = null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _loopbackHost = new LoopbackHost(loopbackHostUri, HtmlResponseOnSuccessFactory, HtmlResponseOnErrorFactory, Log);
            try
            {
                _loopbackHost.LoopbackFilter = LoopbackFilter;
                timeout ??= LoopbackHost.s_defaultTimeout;
#pragma warning disable CS4014
                OnOpenBrowserAsync(targetUri);
#pragma warning restore CS4014
                var request = await _loopbackHost.WaitForCallbackUrlAsync(timeout.Value);
                return await OnOutcomeAsync(request is { }
                    ? Outcome<GenericHttpRequest>.Success(await request.ToGenericHttpRequestAsync())
                    : Outcome<GenericHttpRequest>.Fail(
                        new Exception($"Did not obtain a callback from browser interaction with {targetUri}")));
            }
            catch (Exception ex)
            {
                return Outcome<GenericHttpRequest>.Fail(ex);
            }
            finally
            {
                DisposeLoopbackHost();
            }
        }

        protected virtual Task<Outcome<GenericHttpRequest>> OnOutcomeAsync(Outcome<GenericHttpRequest> outcome)
        {
            return Task.FromResult(outcome);
        }

        protected void DisposeLoopbackHost()
        {
            _loopbackHost?.Dispose();
            _loopbackHost = null;
        }

        internal static Task<LoopbackFilterOutcome> DefaultLoopbackFilter(HttpRequest request)
        {
            return Task.FromResult(
                request.Method == "GET" 
                    ? LoopbackFilterOutcome.Accept 
                    : LoopbackFilterOutcome.RejectAndFail);
        }

        internal static Task<LoopbackFilterOutcome> DefaultLoopbackPatternFilter(HttpRequest request)
        {
            // todo user wait-for regex pattern 
            if (request.Method != HttpMethods.Get)
                return Task.FromResult(LoopbackFilterOutcome.RejectAndFail); 

            return Task.FromResult(
                request.Method == "GET" 
                    ? LoopbackFilterOutcome.Accept 
                    : LoopbackFilterOutcome.RejectAndFail);
        }

        protected abstract Task OnOpenBrowserAsync(Uri uri);

        public void Dispose() => _loopbackHost?.Dispose();

        public LoopbackBrowser(ILog? log)
        {
            Log = log;
            HtmlResponseOnSuccessFactory = getDefaultHtmlResponseOnSuccess();
            HtmlResponseOnErrorFactory = getDefaultHtmlResponseOnError();
        }
    }

    public static class LoopbackBrowserHelper
    {
        public static LoopbackBrowser WithHtmlResponseOnSuccess(this LoopbackBrowser browser, Task<string> htmlFactory)
        {
            browser.HtmlResponseOnSuccessFactory = htmlFactory;
            return browser;
        }
        
        public static LoopbackBrowser WithHtmlResponseOnError(this LoopbackBrowser browser, Task<string> htmlFactory)
        {
            browser.HtmlResponseOnErrorFactory = htmlFactory;
            return browser;
        }
        
        public static LoopbackBrowser WithLoopBackFilter(this LoopbackBrowser browser, LoopbackFilter filter)
        {
            browser.LoopbackFilter = filter;
            return browser;
        }
    }
}