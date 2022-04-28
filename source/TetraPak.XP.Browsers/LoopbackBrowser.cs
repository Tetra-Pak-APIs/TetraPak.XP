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
        LoopbackHost? _loopbackHost;
        
        protected ILog? Log { get; }

        public LoopbackFilter? LoopbackFilter { get; set; }

        public LoopbackBrowser WithLoopBackFilter(LoopbackFilter filter)
        {
            LoopbackFilter = filter;
            return this;
        }

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
        public Task<Outcome<HttpRequest>> GetLoopbackAsync(
            Uri target, 
            Uri loopbackHost, 
            LoopbackFilter? filter = null,
            CancellationToken? cancellationToken = null, 
            TimeSpan? timeout = null)
        {
            try
            {
                WithLoopBackFilter(filter ?? DefaultLoopbackPatternFilter);
                return invokeAsync(target, loopbackHost, cancellationToken ?? CancellationToken.None, timeout);

            }
            catch (Exception ex)
            {
                return Task.FromResult(Outcome<HttpRequest>.Fail(ex));
            }
        }
        
        async Task<Outcome<HttpRequest>> invokeAsync(
            Uri targetUri,
            Uri loopbackHostUri,
            CancellationToken cancellationToken,
            TimeSpan? timeout = null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _loopbackHost = new LoopbackHost(loopbackHostUri, Log);
            try
            {
                _loopbackHost.LoopbackFilter = LoopbackFilter;
                timeout ??= LoopbackHost.s_defaultTimeout;
#pragma warning disable CS4014
                OnOpenBrowserAsync(targetUri);
#pragma warning restore CS4014
                var request = await _loopbackHost.WaitForCallbackUrlAsync(timeout.Value);
                return request is { }
                    ? Outcome<HttpRequest>.Success(request)
                    : Outcome<HttpRequest>.Fail(
                        new Exception($"Did not obtain a callback from browser interaction with {targetUri}"));
            }
            catch (Exception ex)
            {
                return Outcome<HttpRequest>.Fail(ex);
            }
            finally
            {
                _loopbackHost.Dispose();
                _loopbackHost = null;
            }
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
        }
     }
}