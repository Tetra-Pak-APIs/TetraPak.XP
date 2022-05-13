using System.Text.Json.Serialization;

namespace TetraPak.XP.OAuth2.TokenExchange
{
    /// <summary>
    ///   Represents the response from a successful token exchange request
    ///   (see <see cref="ITokenExchangeGrantService.AcquireTokenAsync"/>).
    /// </summary>
    public sealed class TokenExchangeResponseBody
    {
        /// <summary>
        ///   Gets the security token issued by the authorization server in response to the token exchange request.
        /// </summary>
        [JsonPropertyName("access_token")]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string? AccessToken { get; set; }

        /// <summary>
        ///   (optional)<br/>
        ///   A refresh token.
        /// </summary>
        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        /// <summary>
        ///   Gets an identifier for the representation of the issued security token.
        /// </summary>
        [JsonPropertyName("issued_token_type")]
        public string? IssuedTokenType { get; set; }
        
        /// <summary>
        ///   A case-insensitive value specifying the method of using the access token issued,
        ///   as specified in Section 7.1 of [RFC6749].
        /// </summary>
        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        /// <summary>
        ///   (optional, recommended)<br/>
        ///   The validity lifetime, in seconds, of the token issued by the authorization server.
        /// </summary>
        [JsonPropertyName("expires_in")]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string? ExpiresIn { get; set; }
        
        /// <summary>
        ///   (optional)<br/>
        ///   The scope of the issued security token.
        /// </summary>
        [JsonPropertyName("scope")]
        public string? Scope { get; set; }
    }
}