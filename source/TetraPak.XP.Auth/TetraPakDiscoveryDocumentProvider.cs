using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Auth.Abstractions.OIDC;
using TetraPak.XP.Caching;
using TetraPak.XP.Web.Http;

namespace TetraPak.XP.Auth
{
    class TetraPakDiscoveryDocumentProvider : IDiscoveryDocumentProvider
    {
        const string CacheKey = "DiscoveryDocument";
        
        readonly object _syncRoot = new();
        readonly ITetraPakConfiguration _conf;
        readonly IDiscoveryDocumentCache? _cache;
        readonly IHttpClientProvider _httpClientProvider;

        public async Task<Outcome<DiscoveryDocument>> GetDiscoveryDocument(ActorToken? idToken)
        {
            lock (_syncRoot)
            {
                if (DiscoveryDocument.Current is { })
                    return Outcome<DiscoveryDocument>.Success(DiscoveryDocument.Current);
            }
            return await downloadAndSetCurrentAsync(idToken);
        }
        
        async Task<Outcome<DiscoveryDocument>> downloadAndSetCurrentAsync(ActorToken? idToken, bool force = false)
        {
            if (!force && DiscoveryDocument.Current is {})
                return Outcome<DiscoveryDocument>.Success(DiscoveryDocument.Current);
            
            // var idToken = grant?.IdToken;
            DiscoveryDocument? disco = null;
            if (!string.IsNullOrEmpty(idToken?.Value))
            {
                var downloadOutcome = await downloadAsync(idToken!.Value, true);
                if (downloadOutcome)
                {
                    disco = downloadOutcome.Value!;
                }
            }

            if (disco is null)
            {
                if (_cache is null)
                    return Outcome<DiscoveryDocument>.Fail(new Exception("Failed downloading discovery document"));

                var cacheOutcome = await tryLoadCachedAsync();
                if (!cacheOutcome)
                    return Outcome<DiscoveryDocument>.Fail(new Exception("Failed downloading discovery document"));

                disco = cacheOutcome.Value!;
            }

            try
            {
                if (DiscoveryDocument.Current is null || DiscoveryDocument.Current.LastUpdated < disco.LastUpdated)
                    DiscoveryDocument.SetCurrent(disco);

                return Outcome<DiscoveryDocument>.Success(disco);
            }
            catch (Exception ex)
            {
                return Outcome<DiscoveryDocument>.Fail(new Exception($"Failed while deserializing cached discovery document. {ex.Message}", ex));
            }
        }

        async void saveToCache(DiscoveryDocument discoDocument)
        {
            var path = getCachePath();
            if (_cache is { })
            {
                await _cache.CreateOrUpdateAsync(discoDocument, path.Key, path.Repository);
            }
        }
        
        /// <summary>
        ///   Downloads and returns the <seealso cref="DiscoveryDocument"/> found at a Url
        ///   resolved from the specified <paramref name="input"/>.
        /// </summary>
        /// <param name="input">
        ///   Either a URL for the well-known discovery endpoint or a (serialized) JWT token to be used for
        ///   resolving the URL.
        /// </param>
        /// <param name="refreshCurrent">
        ///   (optional; default = <c>false</c>)
        ///   Specifies whether to use the (cached) <seealso cref="DiscoveryDocument.Current"/> value or download
        ///   a new document and set it as the <seealso cref="DiscoveryDocument.Current"/> one.
        /// </param>
        /// <param name="cache">
        ///   (optional)<br/>
        ///   Cache to be use to cache the downloaded <see cref="DiscoveryDocument"/> upon success.
        /// </param>
        /// <returns>
        ///   Please note that when there is a <seealso cref="DiscoveryDocument.Current"/> document already, passing a <c>false</c>
        ///   value will have the method simply return the <seealso cref="DiscoveryDocument.Current"/> one if assigned or, automatically,
        ///   download a new one and then set that as the <seealso cref="DiscoveryDocument.Current"/> document.
        /// </returns>
        async Task<Outcome<DiscoveryDocument>> downloadAsync(
            string input, 
            bool refreshCurrent = false)
        {
            if (DiscoveryDocument.Current is { } && !refreshCurrent)
                return Outcome<DiscoveryDocument>.Success(DiscoveryDocument.Current);

            var resolvedEndpointUrlOutcome = tryResolveUrl(input);
            if (!resolvedEndpointUrlOutcome)
                return Outcome<DiscoveryDocument>.Fail(resolvedEndpointUrlOutcome.Exception!);

            var clientOutcome = await _httpClientProvider.GetHttpClientAsync();
            if (!clientOutcome)
                return Outcome<DiscoveryDocument>.Fail(clientOutcome.Exception!);

            using var client = clientOutcome.Value!;
            using var message = new HttpRequestMessage(HttpMethod.Get, resolvedEndpointUrlOutcome.Value!.Url);
            try
            {
                var response = await client.SendAsync(message);
                if (!response.IsSuccessStatusCode)
                    return Outcome<DiscoveryDocument>.Fail(
                        new HttpServerException(response, $"Error connecting to {input}: {response.ReasonPhrase}"));

                var content = await response.Content.ReadAsStringAsync();
                var discoDocument = JsonSerializer.Deserialize<DiscoveryDocument>(content)!;
                discoDocument.LastUpdated = DateTime.UtcNow;
                DiscoveryDocument.SetCurrent(discoDocument);
                saveToCache(discoDocument);
                return Outcome<DiscoveryDocument>.Success(discoDocument);
            }
            catch (Exception ex)
            {
                return Outcome<DiscoveryDocument>.Fail(new Exception($"Error downloading discovery document from {input}: {ex.Message}", ex));
            }
        }
        
        /// <summary>
        ///   Attempts loading a discovery document from the cache and returns a value to indicate the outcome.
        /// </summary>
        async Task<Outcome<DiscoveryDocument>> tryLoadCachedAsync()
        {
            var path = getCachePath();
            return _cache is {}
                ? await _cache.ReadAsync<DiscoveryDocument>(path.Key, path.Repository)
                : Outcome<DiscoveryDocument>.Fail("Document not cached");
        }

        static RepositoryPath getCachePath() => new(CacheNames.FileCache, CacheKey);

        static Outcome<DiscoveryEndpoint> tryResolveUrl(string input)
        {
            return Uri.TryCreate(input, UriKind.Absolute, out var uri) 
                ? tryParseUrl(uri.AbsoluteUri) 
                : tryResolveUrlFromAssumedJwtToken(input);
        }

        static Outcome<DiscoveryEndpoint> tryResolveUrlFromAssumedJwtToken(string input)
        {
            try
            {
                var jwtToken = new JwtSecurityToken(input);
                return tryParseUrl(jwtToken.Issuer);
            }
            catch (Exception ex)
            {
                return Outcome<DiscoveryEndpoint>.Fail( new Exception($"Cannot resolve discovery endpoint from: \"{input}\"", ex));
            }
        }
        
        /// <summary>
        ///   Parses a URL and turns it into authority and discovery endpoint URL.
        /// </summary>
        /// <param name="input">
        ///   The input.
        /// </param>
        /// <returns>
        ///   
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   Url is malformed.
        /// </exception>
        public static DiscoveryEndpoint ParseUrl(string input)
        {
            var parseOutcome = tryParseUrl(input);
            if (parseOutcome)
                return parseOutcome.Value!;

            throw parseOutcome.Exception!;
        }

        static Outcome<DiscoveryEndpoint> tryParseUrl(string input)
        {
            var success = Uri.TryCreate(input, UriKind.Absolute, out var uri);
            if (success == false)
            {
                var msg = $"Malformed URL: {input}";
                return Outcome<DiscoveryEndpoint>.Fail(new FormatException(msg));
            }

            if (!isValidScheme(uri!))
            {
                var msg = $"Invalid scheme in URL: {input}";
                return Outcome<DiscoveryEndpoint>.Fail(new InvalidOperationException(msg));
            }

            var url = input.RemoveTrailingSlash();
            var authority = url.EndsWith(DiscoveryEndpoint.WellKnownEndpoint, StringComparison.OrdinalIgnoreCase)
                ? url.Substring(0, url.Length - DiscoveryEndpoint.WellKnownEndpoint.Length - 1)
                : url;
            url = url.EndsWith(DiscoveryEndpoint.WellKnownEndpoint, StringComparison.OrdinalIgnoreCase)
                ? url
                : $"{url.EnsurePostfix("/")}{DiscoveryEndpoint.WellKnownEndpoint}";


            return Outcome<DiscoveryEndpoint>.Success(new DiscoveryEndpoint(authority, url));
        }
        
        /// <summary>
        ///   Determines whether the URL uses http or https.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>
        ///   <c>true</c> if [is valid scheme] [the specified URL]; otherwise, <c>false</c>.
        /// </returns>
        static bool isValidScheme(Uri url)
        {
            if (string.Equals(url.Scheme, "http", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(url.Scheme, "https", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
        
        public TetraPakDiscoveryDocumentProvider(
            ITetraPakConfiguration configuration,
            IHttpClientProvider httpClientProvider, 
            IDiscoveryDocumentCache? cache = null)
        {
            _httpClientProvider = httpClientProvider;
            _conf = configuration;
            _cache = cache;
        }
    }
}