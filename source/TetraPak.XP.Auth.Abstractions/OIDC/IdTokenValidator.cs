using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using JsonWebKeySet = TetraPak.XP.Auth.Abstractions.OIDC.JsonWebKeySet;

namespace TetraPak.XP.Auth.Abstractions.OIDC
{
    /// <summary>
    ///   Used to validate a JWT (id) token.
    /// </summary>
    public class IdTokenValidator
    {
        /// <summary>
        ///   Gets or sets the policy used for discovery.
        /// </summary>
        public DiscoveryPolicy DiscoveryPolicy { get; set; }
        
        public async Task<Outcome<ClaimsPrincipal>> ValidateAsync(ActorToken idToken, JwtTokenValidationOptions? options = null)
        {
            try
            {
                var downloadOutcome = await DiscoveryDocument.DownloadAsync(idToken.StringValue);
                if (!downloadOutcome)
                    return Outcome<ClaimsPrincipal>.Fail(downloadOutcome.Exception!);

                var jwksOutcome = await JsonWebKeySet.DownloadAsync(downloadOutcome.Value!.JwksUri);
                if (!jwksOutcome)
                    return Outcome<ClaimsPrincipal>.Fail(jwksOutcome.Exception!);

                options ??= new JwtTokenValidationOptions();
                var parameters = options.ToTokenValidationParameters(
                    new JwtSecurityToken(idToken), 
                    downloadOutcome.Value, 
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
        public IdTokenValidator() : this(new DiscoveryPolicy())
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <seealso cref="IdTokenValidator"/>
        ///   while specifying the discovery policy.
        /// </summary>
        public IdTokenValidator(DiscoveryPolicy discoveryPolicy)
        {
            DiscoveryPolicy = discoveryPolicy;
        }
    }
}