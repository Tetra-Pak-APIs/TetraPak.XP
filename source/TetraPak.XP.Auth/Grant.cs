using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using TetraPak.XP.Auth.OIDC;
using TetraPak.XP.Caching.Abstractions;
using TetraPak.XP.Logging;

namespace TetraPak.XP.Auth
{
    /// <summary>
    ///   Represents the result of an authorization operation.
    /// </summary>
    public class Grant
    {
        UserInformation _userInformation;
        UserInfoLoader _userInfoLoader;
        readonly AuthConfig _authConfig;

        ILog? Log => _authConfig.Log;

        ITimeLimitedRepositories? Cache => _authConfig.Cache;

        /// <summary>
        ///   A collection of tokens (represented as <see cref="TokenInfo"/> objects) returned from the issuer.
        /// </summary>
        public TokenInfo[]? Tokens { get; }

        /// <summary>
        ///   Gets the access token when successful.
        /// </summary>
        public string? AccessToken => Tokens?.FirstOrDefault(i => i.Role == TokenRole.AccessToken)?.TokenValue;

        /// <summary>
        ///   Gets an optional refresh token when successful.
        /// </summary>
        public string? RefreshToken => Tokens?.FirstOrDefault(i => i.Role == TokenRole.RefreshToken)?.TokenValue;

        /// <summary>
        ///   Gets an optional identity token when successful.
        /// </summary>
        public string? IdToken => Tokens?.FirstOrDefault(i => i.Role == TokenRole.IdToken)?.TokenValue;

        /// <summary>
        ///   Gets any provided expiration time when successful.
        /// </summary>
        public DateTime? Expires => Tokens?.FirstOrDefault(i => i.Role == TokenRole.AccessToken)?.Expires;

        /// <summary>
        ///   Indicates whether the authorization is (still) valid.
        /// </summary>
        [Obsolete("The IsValid property should no longer be used. Please use the TokenInfo.IsValidAsync() method instead")]
        public bool IsValid
        {
            get
            {
                var accessToken = Tokens?.FirstOrDefault(i => i.Role == TokenRole.AccessToken);
                if (accessToken is null)
                    return false;

                return !accessToken.Expires.HasValue || accessToken.Expires.Value > DateTime.Now;
            }
        }

        /// <summary>
        ///   Attempts obtaining user information.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        public async Task<Outcome<UserInformation>> TryGetUserInformationAsync()
        {
            Log?.Debug("[GET USER INFORMATION BEGIN]");

            if (AccessToken is null)
            {
                var error = new Exception("Cannot get user information without a valid access token");
                Log?.Warning(error.Message);
                return Outcome<UserInformation>.Fail(error);
            }

            if (_userInformation != null)
            {
                Log?.Debug("User information was cached");
                return Outcome<UserInformation>.Success(_userInformation);
            }

            try
            {
                Log?.Debug("Retrieves user information from API ...");
                var discoDoc = DiscoveryDocument.Current;
                if (discoDoc is null)
                {
                    var gotDiscoDoc = await DiscoveryDocument.TryDownloadAndSetCurrentAsync(this, Cache);
                    if (!gotDiscoDoc)
                    {
                        var error = new Exception("Failed to retrieve the discovery document. Cannot resolve user information endpoint");
                        Log?.Debug($"ERROR: {error.Message}");
                        return Outcome<UserInformation>.Fail(error);
                    }

                    discoDoc = gotDiscoDoc.Value;
                }
            
                _userInfoLoader ??= new UserInfoLoader(AccessToken, discoDoc, Log);
                _userInformation = await _userInfoLoader.AwaitDownloadedAsync();
                Log?.Debug("Successfully received user information from API");
                return Outcome<UserInformation>.Success(_userInformation);
            }
            catch (Exception ex)
            {
                const string Message = "Failed while retrieving user information from API";
                Log?.Error(ex, Message);
                return Outcome<UserInformation>.Fail(Message, ex);
            }
            finally
            {
                Log?.Debug("[GET USER INFORMATION END]");
            }
        }

        public async Task<Outcome<string[]>> TeyGetUserInfoTypes()
        {
            if (AccessToken is null)
            {
                var error = new Exception("Cannot get user information without a valid access token");
                Log?.Warning(error.Message);
                return Outcome<string[]>.Fail(error);
            }
        
            try
            {
                if (_userInformation is { }) 
                    return Outcome<string[]>.Success(_userInformation.Types);
        
                var discoDoc = DiscoveryDocument.Current;
                if (discoDoc is null)
                {
                    var gotDiscoDoc = await DiscoveryDocument.TryDownloadAndSetCurrentAsync(this, Cache);
                    if (!gotDiscoDoc)
                    {
                        var error = new Exception("Failed to retrieve the discovery document. Cannot resolve user information endpoint");
                        Log?.Debug($"ERROR: {error.Message}");
                        return Outcome<string[]>.Fail(error);
                    }
                    discoDoc = gotDiscoDoc.Value;
                }
                _userInfoLoader ??= new UserInfoLoader(AccessToken, discoDoc, Log);
                _userInformation = await _userInfoLoader.AwaitDownloadedAsync();
        
                return Outcome<string[]>.Success(_userInformation.Types);
            }
            catch (Exception ex)
            {
                Log?.Error(ex);
                return Outcome<string[]>.Fail(ex.Message, ex);
            }
        }

        internal Grant(AuthConfig config, params TokenInfo[] tokens)
        {
            _authConfig = config;
            Tokens = tokens;
        }
    }

    /// <summary>
    ///   Carries an individual token and its meta data.
    /// </summary>
    public class TokenInfo
    {
        readonly ValidateTokenDelegate _validateTokenDelegate;
        bool _isValidatedByDelegate;

        /// <summary>
        ///   Gets the actual token as a <see cref="string"/> value.
        /// </summary>
        public string TokenValue { get; }

        /// <summary>
        ///   Gets the token role (see <see cref="TokenRole"/>).
        /// </summary>
        public TokenRole Role { get; }

        /// <summary>
        ///   Gets a expiration date/time, if available.
        /// </summary>
        public DateTime? Expires { get; }

        /// <summary>
        ///   Gets a value that indicates whether the token can be validated (other than just by its longevity).
        /// </summary>
        public bool IsValidatable => _validateTokenDelegate != null;

        /// <summary>
        ///   Validates the token and returns a value to indicate whether it is valid at this point. 
        /// </summary>
        public async Task<bool> IsValidAsync()
        {
            if (isTokenExpired())
                return false;

            if (_validateTokenDelegate is null || _isValidatedByDelegate)
                return true;

            var isValid = await _validateTokenDelegate(TokenValue);
            _isValidatedByDelegate = true;
            return isValid;
        }

        bool isTokenExpired() => Expires.HasValue && Expires.Value <= DateTime.Now;

        internal TokenInfo(string tokenValue, TokenRole role, DateTime? expires = null, ValidateTokenDelegate validateTokenDelegate = null)
        {
            TokenValue = tokenValue;
            Role = role;
            Expires = expires;
            _validateTokenDelegate = validateTokenDelegate;
        }
    }

    class UserInfoLoader
    {
        readonly TaskCompletionSource<UserInformation> _tcs;
        readonly string _accessToken;
        readonly ILog _log;

        void downloadAsync(Uri userInfoUri)
        {
            Task.Run(async () =>
            {
                var request = (HttpWebRequest)WebRequest.Create(userInfoUri);
                request.Method = "GET";
                request.Accept = "*/*";
                request.Headers.Add($"Authorization: Bearer {_accessToken}");

                _log?.DebugWebRequest(request, null);

                try
                {
                    var response = await request.GetResponseAsync();
                    var responseStream = response.GetResponseStream()
                                         ?? throw new Exception("Unexpected error: No response when requesting token.");

                    using (var r = new StreamReader(responseStream))
                    {
                        var text = await r.ReadToEndAsync();

                        _log?.DebugWebResponse(response as HttpWebResponse, text);

                        var dictionary = JsonSerializer.Deserialize<IDictionary<string, object>>(text);
                        _tcs.SetResult(new UserInformation(dictionary));
                    }
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

        public Task<UserInformation> AwaitDownloadedAsync() => _tcs.Task;

        public UserInfoLoader(string accessToken, DiscoveryDocument discoDoc, ILog log)
        {
            _accessToken = accessToken;
            _log = log;
            _tcs = new TaskCompletionSource<UserInformation>();
            downloadAsync(new Uri(discoDoc.UserInformationEndpoint));
        }
    }

    public class UserInformation
    {
        readonly IDictionary<string, object> _dictionary;

        public string[] Types => _dictionary.Keys.ToArray();

        public bool TryGet<T>(string type, out T value)
        {
            if (!_dictionary.TryGetValue(type, out var obj))
            {
                value = default;
                return false;
            }

            if (!(obj is T typedValue)) 
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

    public static class UserInfoTypes
    {
        public const string Subject = "sub";
        public const string UserId = Subject;
        public const string Name = "name";
        public const string FamilyName = "family_name";
        public const string Email = "email";
        public const string Domain = "domain";
    }

    delegate Task<Outcome<string>> ValidateTokenDelegate(string token);

}