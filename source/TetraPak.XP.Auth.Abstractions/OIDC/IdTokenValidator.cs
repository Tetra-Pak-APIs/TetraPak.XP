using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TetraPak.XP.Auth.Abstractions.OIDC
{
    /// <summary>
    ///   Used to validate a JWT (id) token.
    /// </summary>
    public sealed class IdTokenValidator
    {
        readonly IDiscoveryDocumentProvider _discoveryDocumentProvider;

        /// <summary>
        ///   Gets or sets the policy used for discovery.
        /// </summary>
        public DiscoveryPolicy DiscoveryPolicy { get; set; }
        
        public async Task<Outcome<ClaimsPrincipal>> ValidateIdTokenAsync(ActorToken idToken, JwtTokenValidationOptions? options = null)
        {
            if (string.IsNullOrEmpty(idToken))
                return Outcome<ClaimsPrincipal>.Fail("Id token is unassigned");
            
            try
            {
                var discoOutcome = await _discoveryDocumentProvider.GetDiscoveryDocumentAsync(idToken);
                if (!discoOutcome)
                    return Outcome<ClaimsPrincipal>.Fail(discoOutcome.Exception!);

                var disco = discoOutcome.Value!;
                if (string.IsNullOrEmpty(disco.JwksUri))
                    return Outcome<ClaimsPrincipal>.Fail("Discovery document does not specify a JWKS uri");
                    
                var jwksOutcome = await JsonWebKeySet.DownloadAsync(discoOutcome.Value!.JwksUri!);
                if (!jwksOutcome)
                    return Outcome<ClaimsPrincipal>.Fail(jwksOutcome.Exception!);

                options ??= new JwtTokenValidationOptions();
                var parameters = options.ToTokenValidationParameters(
                    new JwtSecurityToken(idToken), 
                    discoOutcome.Value, 
                    jwksOutcome.Value!);
                var handler = new JwtSecurityTokenHandler();
                handler.InboundClaimTypeMap.Clear();
                var user = handler.ValidateToken(idToken, parameters, out _);
                return Outcome<ClaimsPrincipal>.Success(user);
            }
            catch (Exception ex)
            {
                return Outcome<ClaimsPrincipal>.Fail(ex);
            }
        }

        DiscoveryEndpoint validate(DiscoveryEndpoint discoveryEndpoint, string issuer)
        {
            if (!DiscoveryPolicy.ValidateIssuerName) 
                return discoveryEndpoint;
            
            if (DiscoveryPolicy.RequireHttps && !DiscoveryEndpoint.IsSecureScheme(new Uri(discoveryEndpoint.Url), DiscoveryPolicy))
                throw new InvalidOperationException($"Error connecting to {discoveryEndpoint.Url}. HTTPS required.");

            var strategy = DiscoveryPolicy.AuthorityValidationStrategy ?? DiscoveryPolicy.DefaultAuthorityValidationStrategy;
            var issuerValidationResult = strategy.IsIssuerNameValid(issuer, discoveryEndpoint.Authority);
            if (!issuerValidationResult.Success)
                throw  new InvalidOperationException($"Error connecting to {discoveryEndpoint.Url}. {issuerValidationResult.ErrorMessage}.");

            return discoveryEndpoint;
        }

        /// <summary>
        ///   Initializes a new instance of the <seealso cref="IdTokenValidator"/>
        ///   using the default discovery policy.
        /// </summary>
        public IdTokenValidator(IDiscoveryDocumentProvider discoveryDocumentProvider) 
        : this(discoveryDocumentProvider, new DiscoveryPolicy())
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <seealso cref="IdTokenValidator"/>
        ///   while specifying the discovery policy.
        /// </summary>
        public IdTokenValidator(IDiscoveryDocumentProvider discoveryDocumentProvider, DiscoveryPolicy discoveryPolicy)
        {
            _discoveryDocumentProvider = discoveryDocumentProvider;
            DiscoveryPolicy = discoveryPolicy;
        }
    }
}