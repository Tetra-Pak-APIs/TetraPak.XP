using System;
using System.Diagnostics;
using System.Text.Json.Serialization;
using TetraPak.XP.StringValues;

namespace TetraPak.XP.ApplicationInformation
{
    /// <summary>
    ///   Intended mainly for internal use; objects of this class help describe an app.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("{ToString()}")]
    public class ApplicationInfo : IStringValue
    {
        [JsonIgnore]
        public string StringValue { get; }

        /// <summary>
        ///   Gets or sets the current 
        /// </summary>
        public static ApplicationInfo? Current { get; set; }
        
        // [JsonPropertyName("typ")]
        // public string Type { get; } // eg. mobile | desktop | web

        [JsonPropertyName("pfm")]
        public string Platform { get; } // eg. Xamarin | Maui | console | WPF | UWP | AspNet

        [JsonPropertyName("fwk")]
        public string Framework { get; } // eg. ".NETCoreApp,Version=v5.0" 

        [JsonPropertyName("ops")]
        public string OperatingSystem { get; } // eg. Windows / MacOS / Linux / iOS / Android

        [JsonPropertyName("sdk")]
        public string Sdk { get; set; }

        /// <summary>
        ///   Implicitly converts the <see cref="ApplicationInfo"/> object into its textual representation.
        /// </summary>
        public static implicit operator string(ApplicationInfo applicationInfo) => applicationInfo.StringValue;

        public override string ToString() => StringValue;

        string makeStringValue() => $"{Platform} ({Framework}); {OperatingSystem}; .NET sdk=v{Sdk}";

        public ApplicationInfo(ApplicationPlatform platform, string operatingSystem, string sdk)
        {
            Platform = platform.ToString();
            Framework = AppContext.TargetFrameworkName ?? string.Empty;
            OperatingSystem = operatingSystem.ThrowIfUnassigned(nameof(operatingSystem));
            Sdk = sdk.ThrowIfUnassigned(nameof(sdk));
            StringValue = makeStringValue();
        }

        public ApplicationInfo(string framework, string operatingSystem, string sdk)
        {
            Platform = framework.ThrowIfUnassigned(nameof(framework));
            Framework = AppContext.TargetFrameworkName ?? string.Empty;
            OperatingSystem = operatingSystem.ThrowIfUnassigned(nameof(operatingSystem));
            Sdk = sdk.ThrowIfUnassigned(nameof(sdk));
            StringValue = makeStringValue();
        }
    }
}