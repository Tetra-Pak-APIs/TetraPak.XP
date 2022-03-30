using System;
using System.IdentityModel.Tokens.Jwt;

namespace TetraPak.XP.Auth.Abstractions.OIDC
{
    /// <summary>
    /// Represents a URL to a discovery endpoint - parsed to separate the URL and authority
    /// </summary>
    public sealed class DiscoveryEndpoint
    {
        internal const string WellKnownEndpoint = ".well-known/openid-configuration";

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
