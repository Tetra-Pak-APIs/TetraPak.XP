using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Auth.Abstractions.OIDC;
using TetraPak.XP.Caching.Abstractions;
using TetraPak.XP.Logging.Abstractions;
using TetraPak.XP.StringValues;
using TetraPak.XP.Web.Http;
using TetraPak.XP.Web.Http.Debugging;

namespace TetraPak.XP.Identity;

sealed class TetraPakUserInformationService : IUserInformationService
{
    const string CacheRepository = "userInformation";
    
    readonly IHttpClientProvider _httpClientProvider;
    readonly IDiscoveryDocumentProvider _discoDocProvider;
    readonly ITimeLimitedRepositories? _cache;
    readonly ILog? _log;

    /// <inheritdoc />
    public async Task<Outcome<UserInformation>> GetUserInformationAsync(
        Grant grant, 
        GrantOptions? options, 
        LogMessageId? messageId = null)
    {
        if (grant.AccessToken is null)
            return Outcome<UserInformation>.Fail(new InvalidOperationException($"Grant.{nameof(Grant.AccessToken)} cannot be unassigned"));

        var accessToken = grant.AccessToken!;
        options ??= GrantOptions.Default();
        if (options.IsCachingAllowed)
        {
            var value = await getCachedAsync(accessToken);
            if (value is { })
            {
                switch (value)
                {
                    case TaskCompletionSource<UserInformation> cachedTcs:
                    {
                        var userInformation = await cachedTcs.Task;
                        await setCachedAsync(accessToken, userInformation);
                        return Outcome<UserInformation>.Success(userInformation);
                    }

                    case UserInformation userInformation:
                        using (_log?.Section(LogRank.Debug, $"Cached user information was found: {userInformation}"))
                        {
                            _log?.LogDictionary(userInformation.ToDictionary(), LogRank.Debug);
                        }

                        return Outcome<UserInformation>.Success(userInformation);
                }
            }
        }

        _log.Trace("Obtains discovery document");
        var discoOutcome = await _discoDocProvider.GetDiscoveryDocumentAsync(grant.IdToken);
        if (!discoOutcome)
        {
            const string MissingDiscoDocErrorMessage =
                "Could not obtain user information from Tetra Pak's User Information services. " +
                "Failed when downloading discovery document";
            _log.Warning(MissingDiscoDocErrorMessage, messageId);
            return Outcome<UserInformation>.Fail(discoOutcome.Exception!);
        }

        var userInfoEndpoint = discoOutcome.Value!.UserInformationEndpoint!;
        var completionSource = downloadAsync(accessToken, new Uri(userInfoEndpoint), options, messageId);
        if (options.IsCachingAllowed)
        {
            await setCachedAsync(accessToken, completionSource);
        }
        return await completionSource.Task;
    }
    
    
    TaskCompletionSource<Outcome<UserInformation>> downloadAsync(
        ActorToken accessToken, 
        Uri userInfoUri,
        GrantOptions options,
        LogMessageId? messageId = null)
    {
        _log?.Trace($"Calls user info endpoint: {userInfoUri}");
        var tcs = new TaskCompletionSource<Outcome<UserInformation>>();
        Task.Run(async () =>
        {
            using (_log?.Section(LogRank.Trace, "[GET USER INFO BEGIN]"))
            {
                try
                {
                    var clientOutcome = await _httpClientProvider.GetHttpClientAsync();
                    if (!clientOutcome)
                        throw new HttpServerConfigurationException(
                            "Cannot download user information. Failed when obtaining HTTP client (see inner exception)",
                            clientOutcome.Exception);
                        
                    var request = new HttpRequestMessage(HttpMethod.Get, userInfoUri);
                    request.Headers.Accept.ResetTo(new MediaTypeWithQualityHeaderValue("*/*"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Identity);
                    
                    var sb = _log?.IsEnabled(LogRank.Trace) ?? false
                        ? await (await request.ToGenericHttpRequestAsync()).ToStringBuilderAsync(
                            new StringBuilder(), 
                            () => TraceHttpRequestOptions.Default(messageId).WithInitiator(this, HttpDirection.Out))
                        : null;

                    var client = clientOutcome.Value!;
                    var ct = options.CancellationTokenSource?.Token ?? CancellationToken.None;
                    var response = await client.SendAsync(request, ct);
                    if (!response.IsSuccessStatusCode)
                        tcs.SetResult(Outcome<UserInformation>.Fail(new HttpServerException(response, "Failed when downloading user information")));
                    
                    if (sb is { })
                    {
                        await (await response.ToGenericHttpResponseAsync()).ToStringBuilderAsync(sb);
                        _log.Trace(sb.ToString());
                    }

#if NET5_0_OR_GREATER                                             
                    var responseStream = await response.Content.ReadAsStreamAsync(ct)
#else
                    var responseStream = await response.Content.ReadAsStreamAsync()
#endif
                                         ?? throw new Exception("Unexpected error: No response when requesting user information.");

                    using var r = new StreamReader(responseStream);
                    var text = await r.ReadToEndAsync();

                    var objDictionary = JsonSerializer.Deserialize<IDictionary<string, object>>(text);
                    if (objDictionary is null)
                    {
                        tcs.SetResult(Outcome<UserInformation>.Success(
                            new UserInformation(new Dictionary<string, string>())));
                        return;
                    }

                    var dictionary = new Dictionary<string, string>();
                    foreach (var pair in objDictionary)
                    {
                        if (pair.Value is not JsonElement jsonElement)
                            throw new Exception();

                        dictionary[pair.Key] = jsonElement.GetRawText();
                    }

                    var userInformation = new UserInformation(dictionary);
                    tcs.SetResult(Outcome<UserInformation>.Success(userInformation));
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                    tcs.SetException(ex);
                }
                finally
                {
                    _log.Trace("[GET USER INFO END]");
                }
            }
        });
        return tcs;
    }
    
    async Task<object?> getCachedAsync(ActorToken accessToken)
    {
        if (_cache is null)
            return null;

        var outcome = await _cache.ReadAsync<object>(CacheRepository, accessToken.StringValue);
        return outcome
            ? outcome.Value
            : null;
    }
    
    async Task setCachedAsync(ActorToken accessToken, object value)
    {
        if (_cache is null)
            return;

        await _cache.CreateOrUpdateAsync(value, accessToken.StringValue, CacheRepository);
    }

    public TetraPakUserInformationService(
        IHttpClientProvider httpClientProvider,
        IDiscoveryDocumentProvider discoDocProvider,
        ITimeLimitedRepositories? cache = null,
        ILog? log = null)
    {
        _httpClientProvider = httpClientProvider;
        _discoDocProvider = discoDocProvider;
        _cache = cache;
        _log = log;
    }
}