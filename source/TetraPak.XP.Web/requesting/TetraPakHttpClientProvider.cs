using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Logging;
using TetraPak.XP.Web.Diagnostics;

namespace TetraPak.XP.Web
{
    /// <summary>
    ///   A base implementation of the <see cref="IHttpClientProvider"/>.  
    /// </summary>
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class TetraPakHttpClientProvider : IHttpClientProvider, ITetraPakDiagnosticsProvider
    {
        void ITetraPakDiagnosticsProvider.DiagnosticsStartTimer(string timerKey) =>
            GetDiagnostics()?.StartTimer(timerKey);

        long? ITetraPakDiagnosticsProvider.DiagnosticsStopTimer(string timerKey) =>
            GetDiagnostics()?.GetElapsedMs(timerKey);

        /// <summary>
        ///   Returns a <see cref="ServiceDiagnostics"/> object if available; otherwise <c>null</c>. 
        /// </summary>
        protected ServiceDiagnostics? GetDiagnostics() => HttpContext?.GetDiagnostics();

        readonly Func<HttpClientOptions,HttpClient> _singletonClientFactory;
        readonly HttpClientOptions? _singletonClientOptions;
        HttpClient? _singletonClient;
        static readonly List<IHttpClientDecorator> s_decorators = new();
        readonly IAuthorizationService? _authorizationService;

        /// <summary>
        ///   Gets the Tetra Pak integration configuration.
        /// </summary>
        protected TetraPakConfig TetraPakConfig { get; }

        /// <summary>
        ///   Provides access to th current <see cref="HttpContext"/>.
        /// </summary>
        protected HttpContext? HttpContext => TetraPakConfig.AmbientData.HttpContext;

        /// <summary>
        ///   Gets a logging provider.
        /// </summary>
        protected ILog? Logger => TetraPakConfig.Logger;
        
        /// <summary>
        ///   The singleton instance (if applicable) of the <see cref="HttpClient"/>.
        /// </summary>
        protected HttpClient SingletonClient => _singletonClient ??= _singletonClientFactory(_singletonClientOptions!);

        string? getMessageId(bool enforce) => TetraPakConfig.AmbientData.GetMessageId(enforce);
        
        /// <inheritdoc />
        public async Task<Outcome<HttpClient>> GetHttpClientAsync(
            HttpClientOptions? options = null, 
            CancellationToken? cancellationToken = null)
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

            var grantType = options.AuthConfig?.GrantType ?? GrantType.None;
            if (grantType == GrantType.None) 
                return await OnDecorateClient(client, s_decorators.ToArray());
            
            var authOutcome = await authService.AuthorizeAsync(options, cancellationToken);
            if (!authOutcome)
            {
                var exception = HttpServerException.Unauthorized(
                    "Failed to authenticate an HTTP client",
                    authOutcome.Exception);
                Logger.Error(exception, messageId: getMessageId(false));
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
                        Logger.Warning($"Http client decorator {decorator} failed: {outcome.Exception.Message}");
                    }

                    client = outcome.Value ?? client;
                }
                catch (Exception ex)
                {
                    ex = new Exception($"Http client decorator {decorator} crashed (see inner exception)", ex);
                    Logger.Error(ex, messageId: getMessageId(true));
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
        /// <param name="tetraPakConfig">
        ///   The Tetra Pak integration configuration.
        /// </param>
        /// <param name="authorizationService">
        ///   (optional)<br/>
        ///   A (custom) authorization service to be invoked instead of amy default service. 
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
        ///   The <paramref name="tetraPakConfig"/> was <c>null</c>.
        /// </exception>
        public TetraPakHttpClientProvider(
            TetraPakConfig tetraPakConfig,
            IAuthorizationService? authorizationService = null,
            Func<HttpClientOptions,HttpClient>? singletonClientFactory = null, 
            HttpClientOptions? singletonClientOptions = null)
        {
            TetraPakConfig = tetraPakConfig ?? throw new ArgumentNullException(nameof(tetraPakConfig));
            _authorizationService = authorizationService;
            _singletonClientFactory = singletonClientFactory ?? (_ => new SingletonHttpClient());
            _singletonClientOptions = singletonClientOptions;
        }
    }
    
    class SingletonHttpClient : HttpClient
    {
        protected override void Dispose(bool disposing)
        {
            // ignore disposing
        }
    }
}