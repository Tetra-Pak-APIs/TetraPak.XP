using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using TetraPak.XP.Logging;
using TetraPak.XP.Logging.Abstractions;
using TetraPak.XP.Web;
using TetraPak.XP.Web.Http;
using TetraPak.XP.Web.Http.Debugging;

namespace TetraPak.XP.Browsers
{
    sealed class LoopbackHost : IDisposable
    {
        readonly object _syncRoot = new();
        
        internal static readonly TimeSpan DefaultTimeout =
#if DEBUG 
            TimeSpan.FromMinutes(1);
#else        
            TimeSpan.FromMinutes(5);
#endif        

        readonly IWebHost _host;
        readonly TaskCompletionSource<HttpRequest?> _loopbackTcs = new();
        bool _isDisposed;

        public LoopbackFilter? LoopbackFilter { get; set; }

        static int methodNotAllowed() => (int) HttpStatusCode.MethodNotAllowed;

        async Task setResultAsync(HttpContext ctx)
        {
            _loopbackTcs.TrySetResult(ctx.Request);
            
            try
            {
                ctx.Response.StatusCode = 200;
                ctx.Response.ContentType = "text/html";
                await ctx.Response.WriteAsync("<h1>You can now return to the application.</h1>");
                await ctx.Response.Body.FlushAsync();
            }
            catch
            {
                ctx.Response.StatusCode = 400;
                ctx.Response.ContentType = "text/html";
                await ctx.Response.WriteAsync("<h1>Invalid request.</h1>");
                await ctx.Response.Body.FlushAsync();
            }
        }
        
        public Task<HttpRequest?> WaitForCallbackUrlAsync(TimeSpan timeout)
        {
            Task.Run(async () =>
            {
                await Task.Delay(timeout);
                _loopbackTcs.TrySetResult(null!);
            });

            return _loopbackTcs.Task;
        }
        
        public LoopbackHost(Uri loopbackHost, ILog? log)
        {
            try
            {
                var builder = new WebHostBuilder();
                if (string.IsNullOrEmpty(AppContext.BaseDirectory))
                {
                    builder.UseContentRoot(Directory.GetCurrentDirectory());
                }

                var hostUri = loopbackHost.AbsoluteHost();
                _host = builder
                    .UseKestrel()
                    .UseUrls(hostUri)
                    .Configure(app =>
                    {
                        var path = loopbackHost.AbsolutePath;
                        if (!string.IsNullOrEmpty(path))
                        {
                            app.UsePathBase(path.EnsurePrefix('/'));
                        }
                        app.Run(async ctx =>
                        {
                            log.Trace(() =>
                            {
                                var r = ctx.Request;
                                var port = r.Host.Port ?? loopbackHost.Port;
                                var uri = new UriBuilder(r.Scheme, r.Host.Host, port, r.PathBase + r.QueryString).Uri;
                                return uri.ToStringBuilderAsync(ToString(), null, direction:HttpDirection.In).Result.ToString();
                            });
                            
                            var filter = LoopbackFilter ?? LoopbackBrowser.DefaultLoopbackFilter;
                            switch (await filter.Invoke(ctx.Request))
                            {
                                case LoopbackFilterOutcome.Accept:
                                    await setResultAsync(ctx);
                                    return;
                                
                                case LoopbackFilterOutcome.Ignore:
                                    return;
                                
                                case LoopbackFilterOutcome.RejectAndFail:
                                    ctx.Response.StatusCode = methodNotAllowed();
                                    return;
                                
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        });
                    })
                    .Build();
                _host.Start();
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw;
            }
        }
        
        public void Dispose()
        {
            Task.Run(async () =>
            {
                lock (_syncRoot)
                {
                    if (_isDisposed)
                        return;
                }

                _isDisposed = true;
                _loopbackTcs.TrySetCanceled();
                await Task.Delay(500);
                _host.Dispose();
            });
        }
    }
}