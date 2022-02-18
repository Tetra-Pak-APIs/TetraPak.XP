using System.Text.Json.Serialization;
#pragma warning disable CS8618

namespace TetraPak.XP.Auth.DeviceCode
{
    /// <summary>
    ///   Represents the response from a successful client credentials request
    ///   (see <see cref="IDeviceCodeGrantService.AcquireTokenAsync"/>).
    /// </summary>
    // ReSharper disable once UnusedType.Global
    class DeviceCodeAuthCodeResponseBody
    {
        double _interval;
        const double DefaultInterval = 5d;
        
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
        public double Interval
        {
            get => _interval == 0d ? DefaultInterval : _interval;
            set => _interval = value;
        }
    }
}