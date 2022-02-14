using System;
using System.Diagnostics;

namespace TetraPak.XP.Auth
{
    // todo Write unit tests for OAuthApplication
    /// <summary>
    ///   Describes an application to be authorized.
    ///   The type can be expressed in textual format:
    ///   "[&lt;Platform&gt;]; &lt;Environment&gt;; &lt;Client Id&gt;; &lt;Redirect Uri&gt;";
    /// </summary>
    /// <remarks>
    ///   This class is used to easily describe your application to allow the <see cref="Authorization"/>
    ///   api to configure a <see cref="IAuthenticator"/> as you request it. 
    /// </remarks>
    /// <seealso cref="Authorization.GetAuthenticator(AuthConfig,ILog)"/>
    [DebuggerDisplay("{" + nameof(StringValue) + "}")]
    public class AuthApplication
    {
        const string Format = "[<platform>;] [<environment>;] <clientId>; <redirectUri>";
        static readonly char[] s_separators = { ',', ';'};

        /// <summary>
        ///   Gets the application client id.
        /// </summary>
        public string ClientId { get; }

        /// <summary>
        ///   Gets the application redirect <see cref="Uri"/>.
        /// </summary>
        public Uri RedirectUri { get; }

        /// <summary>
        ///   Gets the application redirect <see cref="Environment"/>.
        /// </summary>
        public RuntimeEnvironment Environment { get; }

        /// <summary>
        ///   Gets the application redirect <see cref="RuntimePlatform"/>.
        /// </summary>
        public RuntimePlatform RuntimePlatform { get; }

        /// <summary>
        ///   Gets the value's textual representation.
        /// </summary>
        public string StringValue { get; }

        static bool tryParse(string s, out RuntimePlatform platform, out string clientId, out Uri redirectUri, out RuntimeEnvironment environment, out string? message)
        {
            var split = s.Split(s_separators, StringSplitOptions.RemoveEmptyEntries);
            switch (split.Length)
            {
                case 2:
                    // platform and environment omitted ...
                    platform = default;
                    environment = default;
                    clientId = split[0].Trim();
                    if (!tryParseRedirectUri(split[1], out redirectUri, out var msg))
                        return error(msg, out platform, out clientId, out redirectUri, out environment, out message);
                    
                    message = null;
                    return true;
                
                case 3:
                    // platform or environment omitted ...
                    if (!tryParseEnvironmentOrPlatform(split[0], out msg, out environment, out platform))
                        return error(msg, out platform, out clientId, out redirectUri, out environment, out message);
                    
                    clientId = split[1].Trim();
                    if (!tryParseRedirectUri(split[2], out redirectUri, out msg))
                        return error(msg, out platform, out clientId, out redirectUri, out environment, out message);
                    
                    message = null;
                    return true;
                    
                case 4:
                    // all elements specified ...
                    if (!Enum.TryParse(split[0].Trim(), out platform))
                        return error($"First element was expected to be {typeof(RuntimePlatform)} but was: {split[0].Trim()}", 
                            out platform, out clientId, out redirectUri, out environment, out message);
                        
                    if (!split[1].TryParseAsRuntimeEnvironment(out environment))
                        return error($"Second element was expected to be {typeof(RuntimeEnvironment)} but was: {split[1].Trim()}", 
                            out platform, out clientId, out redirectUri, out environment, out message);
                    
                    if (!tryParseRedirectUri(split[3], out redirectUri, out msg))
                        return error(msg, out platform, out clientId, out redirectUri, out environment, out message);
                    
                    clientId = split[2].Trim();
                    message = null;
                    return true;
                    
                default:
                    return error($"Expected two, three or four elements: {Format}", 
                        out platform, out clientId, out redirectUri, out environment, out message);
            }

            bool tryParseEnvironmentOrPlatform(string element, out string? errorMsg, out RuntimeEnvironment env, out RuntimePlatform plat)
            {
                element = element.Trim();
                errorMsg = null;
                if (Enum.TryParse(element, out plat))
                {
                    env = default;
                    return true;
                }

                if (element.TryParseAsRuntimeEnvironment(out env))
                {
                    plat = default;
                    return true;
                }
                errorMsg = $"First element was expected to be either {typeof(RuntimeEnvironment)} or {typeof(RuntimePlatform)} but was: {element}";
                return false;
            }
            
            bool tryParseRedirectUri(string element, out Uri uri, out string errorMsg)
            {
                errorMsg = null;
                element = element.Trim();
                if (Uri.TryCreate(element, UriKind.Absolute, out uri))
                    return true;

                errorMsg = $"Malformed redirect uri: {element}";
                return false;
            }
            
            bool error(string txt, out RuntimePlatform rtp, out string cid, out Uri ruri, out RuntimeEnvironment env, out string msg)
            {
                rtp = default;
                cid = null;
                ruri = null;
                env = default;
                msg = $"Invalid {typeof(AuthApplication)} value. {txt}";
                return false;
            }
        }

        string buildStringValue() => $"{RuntimePlatform}{s_separators[0]} {Environment}{s_separators[0]} {ClientId}{s_separators[0]} {RedirectUri}";

        /// <summary>
        ///   Implicitly converts a string literal into a <see cref="AuthApplication"/>.
        /// </summary>
        /// <param name="stringValue">
        ///   A string representation of the <see cref="AuthApplication"/> value.
        /// </param>
        /// <returns>
        ///   A <see cref="AuthApplication"/> value.
        /// </returns>
        /// <exception cref="FormatException">
        ///   The <paramref name="stringValue"/> string representation was incorrectly formed.
        /// </exception>
        public static implicit operator AuthApplication(string stringValue) => new(stringValue);

        /// <summary>
        ///   Implicitly converts a <see cref="AuthApplication"/> value into its <see cref="string"/> representation.
        /// </summary>
        /// <param name="value">
        ///   A <see cref="AuthApplication"/> value to be implicitly converted into its <see cref="string"/> representation.
        /// </param>
        /// <returns>
        ///   The <see cref="string"/> representation of <paramref name="value"/>.
        /// </returns>
        public static implicit operator string(AuthApplication value) => value.StringValue;

        /// <inheritdoc />
        public override string ToString() => StringValue;

        #region .  Equality  .

        /// <summary>
        ///   Determines whether this instance and another specified <seealso cref="AuthApplication"/>
        ///   object are semantically equal.
        /// </summary>
        protected bool Equals(AuthApplication other)
        {
            return ClientId == other.ClientId && Equals(RedirectUri, other.RedirectUri) && Environment == other.Environment && RuntimePlatform == other.RuntimePlatform;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AuthApplication) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (ClientId != null ? ClientId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (RedirectUri != null ? RedirectUri.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) Environment;
                hashCode = (hashCode * 397) ^ (int) RuntimePlatform;
                return hashCode;
            }
        }
        #endregion

        /// <summary>
        ///   Initializes an <see cref="AuthApplication"/> instance.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///   The <paramref name="clientId"/> was unassigned.
        /// </exception>
        public AuthApplication(string clientId, Uri redirectUri, RuntimeEnvironment environment, RuntimePlatform runtimePlatform = default)
        {
            RuntimePlatform = runtimePlatform;
            ClientId = clientId.Trim();
            if (string.IsNullOrEmpty(ClientId))
                throw new ArgumentNullException(nameof(clientId));

            RedirectUri = redirectUri;
            Environment = environment;
            StringValue = buildStringValue();
        }

        /// <summary>
        ///   Initializes the value.
        /// </summary>
        /// <param name="stringValue">
        ///   The new value's string representation (will automatically be parsed).
        /// </param>
        /// <exception cref="FormatException">
        ///   The <paramref name="stringValue"/> string representation was incorrectly formed.
        /// </exception>
        [DebuggerStepThrough]
        public AuthApplication(string? stringValue)
        {
            stringValue = stringValue?.Trim();
            if (string.IsNullOrEmpty(stringValue))
                throw new ArgumentNullException(nameof(stringValue));

            if (!tryParse(stringValue!, out var platform, out var clientId, out var redirectUri, out var environment, out var message))
                throw new FormatException($"Invalid {GetType()}. {message}");

            StringValue = stringValue!;
            RuntimePlatform = platform;
            Environment = environment;
            ClientId = clientId;
            RedirectUri = redirectUri;
        }
    }
}