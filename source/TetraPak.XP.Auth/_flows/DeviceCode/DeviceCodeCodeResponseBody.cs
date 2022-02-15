using System.Text.Json.Serialization;
#pragma warning disable CS8618

namespace TetraPak.XP.Auth.DeviceCode
{
    /// <summary>
    ///   Represents the response from a successful client credentials request
    ///   (see <see cref="IDeviceCodeGrantService.AcquireTokenAsync"/>).
    /// </summary>
    // ReSharper disable once UnusedType.Global
    class DeviceCodeCodeResponseBody
    {
/*
        {
            "device_code": "J9U2Cjtq10Mi1sUPwUEckOLZ6YNdijbyg2a9Vor061",
            "user_code": "G47B-RNGY",
            "verification_uri": "https://ssodev.tetrapak.com/as/user_authz.oauth2",
            "verification_uri_complete": "https://ssodev.tetrapak.com/as/user_authz.oauth2?user_code=G47B-RNGY",
            "expires_in": 600,
            "interval": 5
        }
*/
        
        [JsonPropertyName("device_code")]
        public string DeviceCode { get; set; }

        [JsonPropertyName("user_code")]
        public string UserCode { get; set; }

        [JsonPropertyName("verification_uri")]
        public string VerificationUri { get; set; }
        
        [JsonPropertyName("verification_uri_complete")]
        public string VerificationUriComplete { get; set; }

        [JsonPropertyName("expires_in")]
        public double ExpiresIn { get; set; }

        [JsonPropertyName("interval")]
        public double Interval { get; set; }
    }
}