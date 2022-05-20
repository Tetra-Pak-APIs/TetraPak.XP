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
        
        [JsonPropertyName("typ")]
        public string Type { get; } // mobile | desktop | web

        [JsonPropertyName("fwk")]
        public string Framework { get; } // Xamarin | Maui | console | WPF | UWP | AspNet

        [JsonPropertyName("ops")]
        public string OperatingSystem { get; } // Windows / MacOS / Linux / iOS / Android

        [JsonPropertyName("sdk")]
        public string Sdk { get; set; }

        /// <summary>
        ///   Implicitly converts the <see cref="ApplicationInfo"/> object into its textual representation.
        /// </summary>
        public static implicit operator string(ApplicationInfo applicationInfo) => applicationInfo.StringValue;

        public override string ToString() => StringValue;

        string makeStringValue() => $"{Type}({Framework}.{OperatingSystem}); .NET sdk=v{Sdk}";

        public ApplicationInfo(ApplicationType type, ApplicationFramework framework, string operatingSystem, string sdk)
        {
            Type = type.ToString().ToLower();
            Framework = framework.ToString();
            OperatingSystem = operatingSystem.ThrowIfUnassigned(nameof(operatingSystem));
            Sdk = sdk.ThrowIfUnassigned(nameof(sdk));
            StringValue = makeStringValue();
        }

        public ApplicationInfo(ApplicationType type, string framework, string operatingSystem, string sdk)
        {
            Type = type.ToString().ToLower();
            Framework = framework.ThrowIfUnassigned(nameof(framework));
            OperatingSystem = operatingSystem.ThrowIfUnassigned(nameof(operatingSystem));
            Sdk = sdk.ThrowIfUnassigned(nameof(sdk));
            StringValue = makeStringValue();
        }
    }
}