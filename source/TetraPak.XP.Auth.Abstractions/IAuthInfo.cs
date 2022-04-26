namespace TetraPak.XP.Auth.Abstractions
{
    public interface IAuthInfo
    {
        /// <summary>
        ///   Gets the authority endpoint to be used (eg. for Auth Code/OIDC grants).
        /// </summary>
        string AuthorityUri { get; }

        /// <summary>
        ///   Gets the token issuer endpoint to be used (eg. for Auth Code/OIDC grants).
        /// </summary>
        string TokenIssuerUri { get; }

        /// <summary>
        ///   Gets the endpoint to be used for obtaining a code for the OAuth Device Code grant.
        /// </summary>
        string? DeviceCodeIssuerUri { get; }

        /// <summary>
        ///   Gets the redirect URI to be used (eg. for Auth Code/OIDC grants).
        /// </summary>
        public string? RedirectUri { get; }

        /// <summary>
        ///   Gets the URI for the well-known discovery document (used in some OAuth flows, such as OIDC).
        /// </summary>
        public string DiscoveryDocumentUri { get; }

        /// <summary>
        ///   Gets an authorization scope at this configuration level.
        /// </summary>
        GrantScope? OidcScope { get; }

        /// <summary>
        ///   Gets a flag indicating whether state is to be supported during a grant request (eg. Auth Code/OIDC grants)
        /// </summary>
        bool OidcState { get; }

        /// <summary>
        ///   Gets a flag indicating whether a PKCE value is to be supported during a grant request (eg. Auth Code/OIDC grants)
        /// </summary>
        bool OidcPkce { get; }
    }
}