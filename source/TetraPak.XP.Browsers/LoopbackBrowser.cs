using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using TetraPak.XP.Logging;
using TetraPak.XP.Web;

namespace TetraPak.XP.Browsers
{
    /// <summary>
    ///   Implements a a basic abstract interactive browser. 
    /// </summary>
    public abstract class InteractiveBrowser : IInteractiveBrowser
    {
        LoopbackHost? _loopbackHost;
        readonly ILog? _log;

        public LoopbackFilter? LoopbackFilter { get; set; }

        public InteractiveBrowser WithLoopBackFilter(LoopbackFilter filter)
        {
            LoopbackFilter = filter;
            return this;
        }

        public static int GetRandomUnusedPort() // todo Consider moving to more suitable class (eg. "Network" or something similar)
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
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
        public Task<Outcome<HttpRequest>> ReadLoopbackAsync(
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
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                _loopbackHost = new LoopbackHost(loopbackHostUri, _log);
                _loopbackHost.LoopbackFilter = LoopbackFilter;
                timeout ??= LoopbackHost.DefaultTimeout;
                OnOpenBrowser(targetUri);
                var request = await _loopbackHost.WaitForCallbackUrlAsync(timeout.Value);
                return request is {}
                    ? Outcome<HttpRequest>.Success(request)
                    : Outcome<HttpRequest>.Fail(new Exception($"Did not obtain a callback from browser interaction with {targetUri}"));
            }
            catch (Exception ex)
            {
                return Outcome<HttpRequest>.Fail(ex);
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

        protected abstract void OnOpenBrowser(Uri uri);

        public void Dispose() => _loopbackHost?.Dispose();

        public InteractiveBrowser(ILog? log)
        {
            _log = log;
        }
     }
}