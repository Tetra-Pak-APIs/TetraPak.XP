// using System;
// using System.Threading.Tasks;
// using TetraPak.XP.Caching;
// using TetraPak.XP.Logging;
//
// namespace TetraPak.XP.Auth // obsolete
// {
//     /// <summary>
//     ///   A basic token cache that stores instances of <see cref="AuthResult"/>.
//     /// </summary>
//     public class TokenCache : MemoryCache<AuthResult>
//     {
//         readonly bool _isRefreshTokenPersisted;
//         readonly ILog _log;
//         readonly AuthConfig _authConfig;
//
//         /// <summary>
//         ///   Adds tokens (in an <see cref="AuthResult"/> object) to the cache.
//         /// </summary>
//         /// <param name="key">
//         ///   Identifies the tokens (typically a client/app id).
//         /// </param>
//         /// <param name="tokens">
//         ///   An <see cref="AuthResult"/> object containing the tokens.
//         /// </param>
//         /// <param name="replace">
//         ///   (optional; default = <c>false</c>)
//         ///   When set, an existing item will be replaced. When unset, and an item is already 
//         ///   associated with the <paramref name="key"/>, an <see cref="ArgumentException"/>
//         ///   is thrown.
//         /// </param>
//         /// <param name="expires">
//         ///   (optional)<br/>
//         ///   A time when the tokens expire. Please note that <see cref="AuthResult.Expires"/>
//         ///   governs expiration of the access token themselves. This value simply controls
//         ///   how long the result is being cached. Leave unassigned unless all tokens should expire at the
//         ///   same time.
//         /// </param>
//         public override async Task AddAsync(string key, AuthResult tokens, bool replace = false, DateTime? expires = null)
//         {
//             key ??= DefaultKey;
//             tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
//             if (_isRefreshTokenPersisted)
//             {
//                 await SecureStorage.SetAsync(key, tokens.RefreshToken);
//             }
//
//             await Task.FromResult(base.AddAsync(key, tokens, replace, expires));
//         }
//
//         /// <summary>
//         ///   Attempts fetching all tokens from the cache.
//         /// </summary>
//         /// <param name="key">
//         ///   Identifies the tokens (typically a client/app id).
//         /// </param>
//         /// <returns>
//         ///   An <see cref="AuthResult"/> item.
//         /// </returns>
//         public override async Task<Outcome<AuthResult>> TryGetAsync(string key = null)
//         {
//             key ??= DefaultKey;
//             var cached = await base.TryGetAsync(key);
//             if (cached || !_isRefreshTokenPersisted)
//                 return cached;
//
//             // look for persisted refresh token ...
//             var refreshToken = await SecureStorage.GetAsync(key);
//             return refreshToken is null
//                 ? cached
//                 : Outcome<AuthResult>.Success(new AuthResult(_authConfig, _log, new TokenInfo(refreshToken, TokenRole.RefreshToken)));
//         }
//
//         /// <summary>
//         ///   Removes any access token from cache.
//         /// </summary>
//         /// <param name="key">
//         ///   Identifies the token (typically a client/app id).
//         /// </param>
//         public async Task RemoveAccessTokenAsync(string key = null)
//         {
//             await base.RemoveAsync(key ?? DefaultKey);
//         }
//
//         /// <summary>
//         ///   Removes any refresh token from cache.
//         /// </summary>
//         /// <param name="key">
//         ///   Identifies the token (typically a client/app id).
//         /// </param>
//         public async Task RemoveRefreshTokenAsync(string key = null)
//         {
//             key ??= DefaultKey;
//             if (string.IsNullOrWhiteSpace(key))
//                 throw new ArgumentNullException(nameof(key));
//
//             if (await SecureStorage.GetAsync(key) != null)
//             {
//                 SecureStorage.Remove(key);
//             }
//         }
//
//         /// <summary>
//         ///   Removes all tokens from cache.
//         /// </summary>
//         /// <param name="key">
//         ///   Identifies the tokens (typically a client/app id).
//         /// </param>
//         public override async Task RemoveAsync(string key = null)
//         {
//             key ??= DefaultKey;
//             if (string.IsNullOrWhiteSpace(key))
//                 throw new ArgumentNullException(nameof(key));
//
//             await RemoveRefreshTokenAsync(key);
//             await RemoveAccessTokenAsync(key);
//         }
//
//         /// <summary>
//         ///   Initializes the token cache.
//         /// </summary>
//         /// <param name="config">
//         ///   The <see cref="AuthConfig"/> used for retrieving the token.
//         /// </param>
//         /// <param name="isRefreshTokenPersisted">
//         ///   Specifies whether refresh tokens should be persisted.
//         /// </param>
//         /// <param name="log">
//         ///   (optional)
//         ///   Used for logging internal events.
//         /// </param>
//         public TokenCache(AuthConfig config, bool isRefreshTokenPersisted = true, ILog log = null)
//         {
//             _authConfig = config;
//             _isRefreshTokenPersisted = isRefreshTokenPersisted;
//             _log = log;
//         }
//     }
// }
