namespace TetraPak.XP.Auth.Abstractions
{
    /// <summary>
    ///   used to specify an authentication method when communicating with a backend service.
    /// </summary>
    public enum GrantType
    {
        /// <summary>
        ///   The service do not have to authenticate itself when consuming its backend service.
        /// </summary>
        None,

        /// <summary>
        ///   Abbreviation for <see cref="TokenExchange"/>.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        TX,

        /// <summary>
        ///   The service is authenticating itself towards the backend service by exchanging its
        ///   requesting actor's credentials for it own credentials.
        /// </summary>
        TokenExchange = TX,

        /// <summary>
        ///   Abbreviation for <see cref="ClientCredentials"/>.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        CC,

        /// <summary>
        ///   The service is authenticating itself towards the backend service through
        ///   its own client credentials (client id and client secret).
        /// </summary>
        ClientCredentials = CC,
        
        /// <summary>
        ///   Abbreviation for <see cref="AuthorizationCode"/>.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        AC,

        /// <summary>
        ///   The service is authenticating itself towards the backend service using the
        ///   OAuth Authorization Code grant (three legged flow).
        /// </summary>
        AuthorizationCode = AC,

        /// <summary>
        ///   Abbreviation for <see cref="OpenIdConnect"/>.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        OIDC,

        /// <summary>
        ///   The service is authenticating itself towards the backend service using the
        ///   Open ID Connect grant (three legged flow using a well-known discovery document when needed and
        ///   returning an identity token with the access token).
        /// </summary>
        OpenIdConnect = CC,
        
        /// <summary>
        ///   Abbreviation for <see cref="DeviceCode"/>.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        DC,

        /// <summary>
        ///   The service is authenticating itself towards the backend service
        ///   using the Device Code Grant.
        /// </summary>
        DeviceCode = DC,

        /// <summary>
        ///   Abbreviation for <see cref="Automatic"/>.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        AU,
        
        /// <summary>
        ///   Abbreviation for <see cref="Automatic"/>.
        /// </summary>
        Auto = AU,
        
        /// <summary>
        ///   The grant type is automatically resolved.
        ///   Usually, what this means is that there must be an actor and that when it's a human actor the
        ///   service with select the <see cref="TX"/> grant type but if the actor is a (autonomous) service
        ///   the automated resolution delegate will instead authorize with <see cref="CC"/>.
        /// </summary>
        Automatic = AU,
        
        /// <summary>
        ///   The service authentication mechanism is inherited from its parent service configuration.
        /// </summary>
        Inherited
    }
}