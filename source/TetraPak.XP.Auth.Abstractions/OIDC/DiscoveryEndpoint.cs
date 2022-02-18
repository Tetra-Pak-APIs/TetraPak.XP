using System;
using System.IdentityModel.Tokens.Jwt;

namespace TetraPak.XP.Auth.Abstractions.OIDC
{
    /// <summary>
    /// Represents a URL to a discovery endpoint - parsed to separate the URL and authority
    /// </summary>
    public class DiscoveryEndpoint
    {
        const string WellKnownEndpoint = ".well-known/openid-configuration";

        /// <summary>
        /// Gets or sets the authority.
        /// </summary>
        /// <value>
        /// The authority.
        /// </value>
        public string Authority { get; }

        /// <summary>
        /// Gets or sets the discovery endpoint.
        /// </summary>
        /// <value>
        /// The discovery endpoint.
        /// </value>
        public string Url { get; }

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
            var parseOutcome = TryParseUrl(input);
            if (parseOutcome)
                return parseOutcome.Value!;

            throw parseOutcome.Exception!;
        }

        public static Outcome<DiscoveryEndpoint> TryParseUrl(string input)
        {
            var success = Uri.TryCreate(input, UriKind.Absolute, out var uri);
            if (success == false)
            {
                var msg = $"Malformed URL: {input}";
                return Outcome<DiscoveryEndpoint>.Fail(new FormatException(msg));
            }

            if (!IsValidScheme(uri!))
            {
                var msg = $"Invalid scheme in URL: {input}";
                return Outcome<DiscoveryEndpoint>.Fail(new InvalidOperationException(msg));
            }

            var url = input.RemoveTrailingSlash();
            var authority = url.EndsWith(WellKnownEndpoint, StringComparison.OrdinalIgnoreCase)
                ? url.Substring(0, url.Length - WellKnownEndpoint.Length - 1)
                : url;
            url = url.EndsWith(WellKnownEndpoint, StringComparison.OrdinalIgnoreCase)
                ? url
                : $"{url.EnsurePostfix("/")}{WellKnownEndpoint}";


            return Outcome<DiscoveryEndpoint>.Success(new DiscoveryEndpoint(authority, url));
        }

        public static Outcome<DiscoveryEndpoint> TryResolveUrl(string input)
        {
            return Uri.TryCreate(input, UriKind.Absolute, out var uri) 
                ? TryParseUrl(uri.AbsoluteUri) 
                : tryResolveUrlFromAssumedJwtToken(input);
        }

        static Outcome<DiscoveryEndpoint> tryResolveUrlFromAssumedJwtToken(string input)
        {
            try
            {
                var jwtToken = new JwtSecurityToken(input);
                return TryParseUrl(jwtToken.Issuer);
            }
            catch (Exception ex)
            {
                return Outcome<DiscoveryEndpoint>.Fail( new Exception($"Cannot resolve discovery endpoint from: \"{input}\"", ex));
            }
        }

        /// <summary>
        /// Determines whether the URL uses http or https.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>
        ///   <c>true</c> if [is valid scheme] [the specified URL]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValidScheme(Uri url)
        {
            if (string.Equals(url.Scheme, "http", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(url.Scheme, "https", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
        
        /// <summary>
        ///   Determines whether a url uses a secure scheme according to the policy.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="policy">The policy.</param>
        /// <returns>
        ///   <c>true</c> if [is secure scheme] [the specified URL]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSecureScheme(Uri url, DiscoveryPolicy policy)
        {
            if (policy.RequireHttps != true) 
                return true;

            if (policy.AllowHttpOnLoopback != true)
                return string.Equals(url.Scheme, "https", StringComparison.OrdinalIgnoreCase);
            
            var hostName = url.DnsSafeHost;
            foreach (var address in policy.LoopbackAddresses)
            {
                if (string.Equals(hostName, address, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return string.Equals(url.Scheme, "https", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscoveryEndpoint"/> class.
        /// </summary>
        /// <param name="authority">The authority.</param>
        /// <param name="url">The discovery endpoint URL.</param>
        public DiscoveryEndpoint(string authority, string url)
        {
            Authority = authority;
            Url = url;
        }
    }
}
