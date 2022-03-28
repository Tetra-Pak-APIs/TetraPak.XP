using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TetraPak.XP.Caching;
using TetraPak.XP.Caching.Abstractions;

namespace TetraPak.XP.Auth.Abstractions.OIDC
{
    // ReSharper disable once ClassNeverInstantiated.Global
    /// <summary>
    ///   A discovery document obtained from a well-known OIDC endpoint.  
    /// </summary>
    public class DiscoveryDocument
    {
        /// <summary>
        ///   Gets the cached <see cref="DiscoveryDocument"/>, if available.
        /// </summary>
        /// <seealso cref="DownloadAsync"/>
        public static DiscoveryDocument? Current { get; private set; }

        /// <summary>
        ///   Gets a value indicating when the document was last updated from the discovery endpoint.
        ///   The time is given as UTC.
        /// </summary>
        public DateTime LastUpdated { get; internal set; }

        /// <summary>
        ///   Get the token issuer host base URL. 
        /// </summary>
        [JsonPropertyName("issuer")]
        public string? Issuer { get; set; }

        /// <summary>
        ///   Endpoint that validates all authorization requests.
        /// </summary>
        [JsonPropertyName("authorization_endpoint")]
        public string? AuthorizationEndpoint { get; set; }        
        
        /// <summary>
        ///   URL of the token endpoint. After a client has received an authorization code,
        ///   that code is presented to the token endpoint and exchanged for an identity token,
        ///   an access token, and a refresh token.
        /// </summary>
        [JsonPropertyName("token_endpoint")]
        public string? TokenEndpoint { get; set; }        
         
        /// <summary>
        ///   URL of the user information endpoint. 
        /// </summary>
        [JsonPropertyName("userinfo_endpoint")]
        public string? UserInformationEndpoint { get; set; }        

        /// <summary>
        ///   URL of the JSON Web Key Set.
        ///   This set is a collection of JSON Web Keys, a standard method for representing cryptographic
        ///   keys in a JSON structure. For Hosted Login, that collection consists of the public keys used
        ///   to verify the signatures of the identity tokens issued by the authorization server.
        /// </summary>
        [JsonPropertyName("jwks_uri")]
        public string? JwksUri { get; set; }

        /// <summary>
        ///   Specifies the way the authorization server responds after a user successfully authenticates.
        /// </summary>
        [JsonPropertyName("response_types_supported")]
        public IEnumerable<string>? ResponseTypesSupported { get; set; }
        
        /// <summary>
        ///   JSON array containing a list of the supported Subject Identifier types. 
        /// </summary>
        [JsonPropertyName("subject_types_supported")]
        public IEnumerable<string>? SubjectTypesSupported { get; set; }

        /// <summary>
        ///    JSON array containing a list of the scopes that the authorization server supports. 
        /// </summary>
        [JsonPropertyName("scopes_supported")]
        public IEnumerable<string>? ScopesSupported { get; set; }

        /// <summary>
        ///   Specifies the different ways that a client can be granted an access token and, as a result,
        ///   can be given access to specific resources. 
        /// </summary>
        [JsonPropertyName("grant_types_supported")]
        public IEnumerable<string>? GrantTypesSupported { get; set; }
        
        /// <summary>
        ///   JSON array consisting of all the JSON Web Signature algorithms that can be used for signing
        ///   JSON Web Tokens, such as SHA1 or SHA256. 
        /// </summary>
        [JsonPropertyName("id_token_signing_alg_values_supported")]
        public IEnumerable<string>? IdTokenSigningAlgValuesSupported { get; set; }

        internal static void SetCurrent(DiscoveryDocument? discoDocument)
        {
            Current = discoDocument;
        }
    }
}