using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using TetraPak.XP.Auth.Abstractions.OIDC;
using TetraPak.XP.Logging;

namespace TetraPak.XP.Auth.Abstractions
{
    /// <summary>
    ///   Represents the result of an authorization operation.
    /// </summary>
    public class Grant // todo make Grant serializable
    {
        readonly Dictionary<string, object> _tags = new();

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public static TimeSpan SubtractFromExpires { get; set; } = TimeSpan.FromSeconds(2); // todo make configurable?
        
        /// <summary>
        ///   A collection of tokens (represented as <see cref="TokenInfo"/> objects) returned from the issuer.
        /// </summary>
        public TokenInfo[]? Tokens { get; }

        /// <summary>
        ///   Gets the access token when successful.
        /// </summary>
        public ActorToken? AccessToken => Tokens?.FirstOrDefault(i => i.Role == TokenRole.AccessToken)?.Token;

        /// <summary>
        ///   Gets an optional refresh token when successful.
        /// </summary>
        public ActorToken? RefreshToken => Tokens?.FirstOrDefault(i => i.Role == TokenRole.RefreshToken)?.Token;

        /// <summary>
        ///   Gets an optional identity token when successful.
        /// </summary>
        public ActorToken? IdToken => Tokens?.FirstOrDefault(i => i.Role == TokenRole.IdToken)?.Token;

        /// <summary>
        ///   Gets any provided expiration time when successful.
        /// </summary>
        public DateTime? Expires => Tokens?.FirstOrDefault(i => i.Role == TokenRole.AccessToken)?.Expires;

        /// <summary>
        ///   Gets the granted scope as a <see cref="MultiStringValue"/>.
        /// </summary>
        public MultiStringValue? Scope { get; set; }
        
        internal T? GetValue<T>(string key, T? useDefault) 
            =>
            _tags.TryGetValue(key, out var obj) && obj is T tv ? tv : useDefault;

        internal void SetValue(string key, object value) => _tags[key] = value;

        internal void SetFlag(string key) => _tags[key] = true;

        internal bool IsFlagSet(string key) => GetValue(key, false);

        internal Grant Clone(TimeSpan remainingLifeSpan) 
        {
            var tokens = new List<TokenInfo>();
            if (!(Tokens?.Any() ?? false))
                return new Grant(Array.Empty<TokenInfo>());
                
            foreach (var token in Tokens)
            {
                if (token.Role == TokenRole.AccessToken)
                {
                    tokens.Add(token.Clone(DateTime.UtcNow.Add(remainingLifeSpan.Subtract(SubtractFromExpires))));
                    continue;
                }
                tokens.Add(token.Clone(null));
            }

            return new Grant(tokens.ToArray());
        }
        
        /// <summary>
        ///   Gets a value indicating whether the <see cref="Grant"/> is expired. 
        /// </summary>
        public bool IsExpired => Expires <= DateTime.UtcNow;

        public Grant(params TokenInfo[] tokens)
        { 
            Tokens = tokens;
        }
    }

    /// <summary>
    ///   Carries an individual token and its meta data.
    /// </summary>
    public class TokenInfo // todo make TokenInfo serializable
    {
        readonly TokenValidationDelegate? _tokenValidationDelegate;
        bool _isValidatedByDelegate;

        /// <summary>
        ///   Gets the actual token as a <see cref="string"/> value.
        /// </summary>
        public ActorToken Token { get; }

        /// <summary>
        ///   Gets the token role (see <see cref="TokenRole"/>).
        /// </summary>
        public TokenRole Role { get; }

        /// <summary>
        ///   Gets a expiration date/time, if available.
        /// </summary>
        public DateTime? Expires { get; }
        
        internal TokenInfo Clone(DateTime? expires) => new(Token, Role, expires);

        /// <summary>
        ///   Gets a value that indicates whether the token can be validated (other than just by its longevity).
        /// </summary>
        public bool IsValidatable => _tokenValidationDelegate != null;

        /// <summary>
        ///   Validates the token and returns a value to indicate whether it is valid at this point. 
        /// </summary>
        public async Task<bool> IsValidAsync()
        {
            if (isTokenExpired())
                return false;

            if (_tokenValidationDelegate is null || _isValidatedByDelegate)
                return true;

            var isValid = await _tokenValidationDelegate(Token);
            _isValidatedByDelegate = true;
            return isValid;
        }

        bool isTokenExpired() => Expires.HasValue && Expires.Value <= DateTime.Now;

        //[JsonConstructor]
        public TokenInfo(
            ActorToken token, 
            TokenRole role, 
            DateTime? expires)
        {
            Token = token ?? throw new ArgumentNullException(nameof(token));
            Role = role;
            Expires = expires;
        }

        internal TokenInfo(
            ActorToken token, 
            TokenRole role, 
            DateTime? expires = null, 
            TokenValidationDelegate? tokenValidationDelegate = null)
        : this(token, role, expires)
        {
            _tokenValidationDelegate = tokenValidationDelegate;
        }
    }

    class UserInfoLoader
    {
        readonly TaskCompletionSource<Outcome<UserInformation>> _tcs;
        readonly ActorToken? _accessToken;
        readonly ILog? _log;
    
        void downloadAsync(Uri userInfoUri)
        {
            if (_accessToken is null)
                _tcs.SetResult(Outcome<UserInformation>.Fail(new Exception("No access token was provided")));
            
            Task.Run(async () =>
            {
                var request = (HttpWebRequest)WebRequest.Create(userInfoUri);
                request.Method = "GET";
                request.Accept = "*/*";
                request.Headers.Add($"Authorization: Bearer {_accessToken}");
    
                _log.DebugWebRequest(request, null);
    
                try
                {
                    var response = await request.GetResponseAsync();
                    var responseStream = response.GetResponseStream()
                                         ?? throw new Exception("Unexpected error: No response when requesting token.");

                    using var r = new StreamReader(responseStream);
                    var text = await r.ReadToEndAsync();
    
                    _log?.DebugWebResponse(response as HttpWebResponse, text);
    
                    var dictionary = JsonSerializer.Deserialize<IDictionary<string, object>>(text)!;
                    _tcs.SetResult(Outcome<UserInformation>.Success(new UserInformation(dictionary)));
                }
                catch (Exception ex)
                {
                    _log?.Error(ex);
                    _tcs.SetException(ex);
                }
                finally
                {
                    _log?.Debug("[GET USER INFO END]");
                }
            });
        }
    
        public Task<Outcome<UserInformation>> AwaitDownloadedAsync() => _tcs.Task;
    
        public UserInfoLoader(ActorToken? accessToken, DiscoveryDocument discoDoc, ILog? log)
        {
            _accessToken = accessToken;
            _log = log;
            _tcs = new TaskCompletionSource<Outcome<UserInformation>>();
            downloadAsync(new Uri(discoDoc.UserInformationEndpoint!));
        }
    }

    public class UserInformation
    {
        readonly IDictionary<string, object> _dictionary;
    
        public string[] Types => _dictionary.Keys.ToArray();
    
        public bool TryGet<T>(string type, out T? value)
        {
            if (!_dictionary.TryGetValue(type, out var obj))
            {
                value = default;
                return false;
            }
    
            if (obj is not T typedValue) 
                throw new NotImplementedException();
            
            value = typedValue;
            return true;
    
            // todo Cast from Json Token to requested value.
            // todo Also replace Json Token with converted value to avoid converting twice
        }
    
        public UserInformation(IDictionary<string, object> dictionary)
        {
            _dictionary = dictionary;
        }
    }

    delegate Task<Outcome<ActorToken>> TokenValidationDelegate(ActorToken token);

    public static class GrantHelper
    {
        internal static Grant ForAuthCode(this Grant self,
            ActorToken accessToken, 
            DateTime expires,
            ActorToken? refreshToken, 
            ActorToken? idToken)
        {
            var tokens = new List<TokenInfo>(new []
            {
                new TokenInfo(accessToken, TokenRole.AccessToken, expires)
            });
            if (refreshToken is { })
            {
                tokens.Add(new TokenInfo(refreshToken, TokenRole.RefreshToken));
            }
            if (idToken is {})
            {
                tokens.Add(new TokenInfo(idToken, TokenRole.IdToken));
            }
            return new Grant(tokens.ToArray());
        }
        
        internal static Grant ForClientCredentials(this Grant self, ActorToken accessToken, DateTime expires)
        {
            return new Grant(new TokenInfo(accessToken, TokenRole.AccessToken, expires));
        }

        // internal static Grant ForDeviceCode(this Grant self, obsolete
        //     ActorToken accessToken, 
        //     DateTime expires,
        //     ActorToken? refreshToken, 
        //     ActorToken? idToken)
        // {
        //     var tokens = new List<TokenInfo>(new []
        //     {
        //         new TokenInfo(accessToken, TokenRole.AccessToken, expires)
        //     });
        //     if (refreshToken is { })
        //     {
        //         tokens.Add(new TokenInfo(refreshToken, TokenRole.RefreshToken));
        //     }
        //     if (idToken is {})
        //     {
        //         tokens.Add(new TokenInfo(idToken, TokenRole.IdToken));
        //     }
        //     return new Grant(tokens.ToArray());
        // }
        
        // /// <summary>
        // ///   Attempts obtaining user information.
        // /// </summary>
        // /// <returns></returns>
        // /// <remarks>
        // /// </remarks>
        // public static async Task<Outcome<UserInformation>> TryGetUserInformationAsync(
        //     this Grant grant,
        //     ITimeLimitedRepositories? cache, 
        //     ILog? log = null)
        // {
        //     const string KeyUserInfo = "__userInfo"; 
        //     
        //     log?.Debug("[GET USER INFORMATION BEGIN]");
        //
        //     if (grant.AccessToken is null)
        //     {
        //         var error = new Exception("Cannot get user information without a valid access token");
        //         log?.Warning(error.Message);
        //         return Outcome<UserInformation>.Fail(error);
        //     }
        //
        //     var cachedUserInfo = grant.GetValue<UserInformation>(KeyUserInfo, null!);
        //     if (cachedUserInfo != null)
        //     {
        //         log?.Debug("User information was cached");
        //         return Outcome<UserInformation>.Success(cachedUserInfo);
        //     }
        //
        //     try
        //     {
        //         log?.Debug("Retrieves user information from API ...");
        //         var discoDoc = DiscoveryDocument.Current;
        //         if (discoDoc is null)
        //         {
        //             var gotDiscoDoc = await DiscoveryDocument.TryDownloadAndSetCurrentAsync(grant, cache);
        //             if (!gotDiscoDoc)
        //             {
        //                 var error = new Exception("Failed to retrieve the discovery document. Cannot resolve user information endpoint");
        //                 log?.Debug($"ERROR: {error.Message}");
        //                 return Outcome<UserInformation>.Fail(error);
        //             }
        //
        //             discoDoc = gotDiscoDoc.Value!;
        //         }
        //     
        //         var userInfoLoader = new UserInfoLoader(grant.AccessToken, discoDoc, log);
        //         var userInfoOutcome = await userInfoLoader.AwaitDownloadedAsync();
        //         if (!userInfoOutcome)
        //         {
        //             log?.Warning($"Failed when downloading user information. {userInfoOutcome.Message}");
        //             
        //         }
        //         log?.Debug("Successfully received user information from API");
        //         grant.SetValue(KeyUserInfo, userInfoOutcome);
        //         return Outcome<UserInformation>.Success(userInfoOutcome.Value!);
        //     }
        //     catch (Exception ex)
        //     {
        //         var message = $"Failed while retrieving user information from API. {ex.Message}";
        //         log?.Error(ex, message);
        //         return Outcome<UserInformation>.Fail(new Exception(message, ex));
        //     }
        //     finally
        //     {
        //         log?.Debug("[GET USER INFORMATION END]");
        //     }
        // }
    }
}