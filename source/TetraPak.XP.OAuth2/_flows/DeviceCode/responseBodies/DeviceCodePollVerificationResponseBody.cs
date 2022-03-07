using System.Text.Json.Serialization;

namespace TetraPak.XP.OAuth2.DeviceCode
{
    /// <summary>
    ///   Represents the response from a successful client credentials request
    ///   (see <see cref="IDeviceCodeGrantService.AcquireTokenAsync"/>).
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    class DeviceCodePollVerificationResponseBody
    {
        internal const string KeyExpiresIn = "expires_in";
        
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName(KeyExpiresIn)]
        public string? ExpiresIn { get; set; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("scope")]
        public string? Scope { get; set; }

        public DeviceCodePollVerificationResponseBody()
        {
            AccessToken = null!;
        }
    }
}