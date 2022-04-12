using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Logging.Abstractions;
using TetraPak.XP.StringValues;

namespace TetraPak.XP.Web.Http
{
    /// <summary>
    ///   A base implementation of the <see cref="IHttpClientProvider"/>.  
    /// </summary>
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class TetraPakHttpClientProvider : IHttpClientProvider //, ITetraPakDiagnosticsProvider todo
    {
        // void ITetraPakDiagnosticsProvider.DiagnosticsStartTimer(string timerKey) => todo
        //     GetDiagnostics()?.StartTimer(timerKey);
        //
        // long? ITetraPakDiagnosticsProvider.DiagnosticsStopTimer(string timerKey) =>
        //     GetDiagnostics()?.GetElapsedMs(timerKey);

        // /// <summary>
        // ///   Returns a <see cref="ServiceDiagnostics"/> object if available; otherwise <c>null</c>.  todo
        // /// </summary>
        // protected ServiceDiagnostics? GetDiagnostics() => HttpContext?.GetDiagnostics();

        readonly Func<HttpClientOptions,HttpClient> _singletonClientFactory;
        readonly HttpClientOptions? _singletonClientOptions;
        HttpClient? _singletonClient;
        static readonly List<IHttpClientDecorator> s_decorators = new();
        readonly IAuthorizationService? _authorizationService;
        readonly ITetraPakConfiguration? _tetraPakConfig;
        readonly ILog? _log;

        /// <summary>
        ///   The singleton instance (if applicable) of the <see cref="HttpClient"/>.
        /// </summary>
        HttpClient SingletonClient => _singletonClient ??= _singletonClientFactory(_singletonClientOptions!);

        LogMessageId? getMessageId(HttpContext client, bool enforce) 
            =>
              client.Request.GetMessageId(_tetraPakConfig, enforce);

        LogMessageId? getMessageId(HttpClient client, bool enforce) 
            =>
                client.GetMessageId(_tetraPakConfig, enforce);
        
        /// <inheritdoc />
        public async Task<Outcome<HttpClient>> GetHttpClientAsync(
            HttpClientOptions? options = null, 
             AuthContext? authContext = null)
        {
            var transient = options?.IsClientTransient ?? true;
            var client = transient
                ? options?.MessageHandler is {} 
                    ? new HttpClient(options.MessageHandler) 
                    : new HttpClient()
                : SingletonClient;

            if (options?.AuthorizationService is null) 
                return await OnDecorateClient(client, s_decorators.ToArray());

            var authService = options.AuthorizationService ?? _authorizationService;
            if (authService is null)
                return Outcome<HttpClient>.Fail(
                    new HttpServerConfigurationException($"Cannot authorize client. A {nameof(IAuthorizationService)} is not available"));

            var grantType = authContext?.GrantType ?? GrantType.None;
            if (grantType == GrantType.None) 
                return await OnDecorateClient(client, s_decorators.ToArray());

            if (authContext is null)
                return await OnDecorateClient(client, s_decorators.ToArray());
            
            var authOutcome = await authService.AuthorizeAsync(authContext);
            if (!authOutcome)
            {
                var exception = HttpServerException.Unauthorized(
                    "Failed to authenticate an HTTP client",
                    authOutcome.Exception);
                _log.Error(exception, messageId: getMessageId(client, false));
                return Outcome<HttpClient>.Fail(exception);
            }

            var token = authOutcome.Value;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.Identity);

            return await OnDecorateClient(client, s_decorators.ToArray());
        }

        /// <summary>
        ///   Called internally (from <see cref="GetHttpClientAsync"/>) to decorate the produced client before
        ///   it gets consumed.
        /// </summary>
        /// <param name="client">
        ///   The <see cref="HttpClient"/> to be decorated.
        /// </param>
        /// <param name="decorators">
        ///   All decorators to be invoked.
        /// </param>
        protected virtual async Task<Outcome<HttpClient>> OnDecorateClient(HttpClient client, IHttpClientDecorator[] decorators)
        {
            for (var i = 0; i < decorators.Length; i++)
            {
                var decorator = decorators[i];
                try
                {
                    var outcome = await decorator.DecorateAsync(client);
                    if (!outcome)
                    {
                        _log.Warning($"Http client decorator {decorator} failed: {outcome.Exception!.Message}");
                    }

                    client = outcome.Value ?? client;
                }
                catch (Exception ex)
                {
                    ex = new Exception($"Http client decorator {decorator} crashed (see inner exception)", ex);
                    _log.Error(ex, messageId: getMessageId(client, true));
                    return Outcome<HttpClient>.Fail(ex);
                }
            }
            return Outcome<HttpClient>.Success(client);
        }

        /// <summary>
        ///   Adds a custom <see cref="HttpClient"/> decorator.
        /// </summary>
        /// <remarks>
        ///   All registered client decorators will be automatically invoked when a client is being requested
        ///   to allow applying custom logic before the client is being consumed.  
        /// </remarks>
        public static void AddDecorator(IHttpClientDecorator decorator)
        {
            if (s_decorators.Contains(decorator))
                throw new InvalidOperationException("Http Client decorator was already added");
            
            s_decorators.Add(decorator);
        }

        /// <summary>
        ///   Initializes the <see cref="TetraPakHttpClientProvider"/>.
        /// </summary>
        /// <param name="tetraPakConfiguration">
        ///   (optional)<br/>
        ///   A Tetra Pak integration configuration.
        /// </param>
        /// <param name="authorizationService">
        ///   (optional)<br/>
        ///   A (custom) authorization service to be invoked instead of amy default service. 
        /// </param>
        /// <param name="log">
        ///   (optional)<br/>
        ///   A logging abstraction to enabled service diagnostics.
        /// </param>
        /// <param name="singletonClientFactory">
        ///   (optional)<br/>
        ///   A custom factory to be used for producing singleton <see cref="HttpClient"/>s. 
        /// </param>
        /// <param name="singletonClientOptions">
        ///   (optional)<br/>
        ///   Options to be used by a custom <paramref name="singletonClientFactory"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   The <paramref name="tetraPakConfiguration"/> was <c>null</c>.
        /// </exception>
        public TetraPakHttpClientProvider(
            ITetraPakConfiguration? tetraPakConfiguration = null,
            IAuthorizationService? authorizationService = null,
            ILog? log = null,
            Func<HttpClientOptions,HttpClient>? singletonClientFactory = null, 
            HttpClientOptions? singletonClientOptions = null)
        {
            _tetraPakConfig = tetraPakConfiguration;
            _log = log;
            _authorizationService = authorizationService;
            _singletonClientFactory = singletonClientFactory ?? (_ => new SingletonHttpClient());
            _singletonClientOptions = singletonClientOptions;
        }
    }

    sealed class SingletonHttpClient : HttpClient
    {
        protected override void Dispose(bool disposing)
        {
            // ignore disposing
        }
    }
}