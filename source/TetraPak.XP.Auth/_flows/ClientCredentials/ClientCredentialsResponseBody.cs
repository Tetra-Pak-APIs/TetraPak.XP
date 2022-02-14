using System.Text.Json.Serialization;

namespace TetraPak.XP.Auth.ClientCredentials
{
    /// <summary>
    ///   Represents the response from a successful client credentials request
    ///   (see <see cref="IClientCredentialsGrantService.AcquireTokenAsync"/>).
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    class ClientCredentialsResponseBody
    {
        [JsonPropertyName("access_token")]
#pragma warning disable CS8618
        public string AccessToken { get; set; }
#pragma warning restore CS8618

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public string? ExpiresIn { get; set; }

        [JsonPropertyName("scope")]
        public string? Scope { get; set; }
    }
}