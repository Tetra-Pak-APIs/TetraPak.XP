using TetraPak.XP.Configuration;

namespace TetraPak.XP.Auth.Abstractions
{
    /// <summary>
    ///   Default/fallback values for authorization scenarios.
    /// </summary>
    public static class TetraPakAuthDefaults
    {
        // internal const RuntimeEnvironment RuntimeEnvironment = XP.RuntimeEnvironment.Production;
        internal const string ProductionDomain = "https://api.tetrapak.com";
        internal const string MigrationDomain = "https://api-mig.tetrapak.com";
        internal const string DevelopmentDomain = "https://api-dev.tetrapak.com";
        internal const string SandboxDomain = "https://api-sb.tetrapak.com";
        internal const string DefaultAuthorityPath = "/oauth2/authorize";
        internal const string DefaultTokenIssuerPath = "/oauth2/token";
        internal const string DefaultDeviceCodeIssuerPath = "/oauth2/device_authorization";
        
        /// <summary>
        ///   Gets or sets a global default value as a fallback when
        ///   <see cref="IAuthConfiguration.OidcState"/> is not configured.
        /// </summary>
        public static bool OidcState { get; set; } = true;

        /// <summary>
        ///   Gets or sets a global default value as a fallback when
        ///   <see cref="IAuthConfiguration.OidcPkce"/> is not configured.
        /// </summary>
        public static bool OidcPkce { get; set; } = true;

        /// <summary>
        ///   Gets or sets a global default value as a fallback when
        ///   <see cref="IAuthConfiguration.OidcPkce"/> is not configured.
        /// </summary>
        public static GrantScope OidcScope { get; set; } = GrantScope.Empty;

        /// <summary>
        ///   Gets or sets a global default value as a fallback when
        ///   <see cref="IAuthConfiguration.IsCaching"/> is not configured.
        /// </summary>
        public static bool IsCaching { get; set; } = true;
        
        

        public static string Domain(RuntimeEnvironment? runtimeEnvironment)
        {
            return runtimeEnvironment switch
            {
                RuntimeEnvironment.Production => ProductionDomain,
                RuntimeEnvironment.Migration => MigrationDomain,
                RuntimeEnvironment.Development => DevelopmentDomain,
                RuntimeEnvironment.Sandbox => SandboxDomain,
                RuntimeEnvironment.Unknown => throw error(),
                _ => throw error()
            };
            
            ConfigurationException error()
                => new($"Could not resolve authority domain from runtime environment '{runtimeEnvironment}'");
        }

        public static string AuthorityUri(RuntimeEnvironment? runtimeEnvironment) 
            => $"{Domain(runtimeEnvironment)}{DefaultAuthorityPath}";
        
        public static string TokenIssuerUri(RuntimeEnvironment? runtimeEnvironment) 
            => $"{Domain(runtimeEnvironment)}{DefaultTokenIssuerPath}";

        public static string DeviceCodeIssuerUri(RuntimeEnvironment? runtimeEnvironment) 
            => $"{Domain(runtimeEnvironment)}{DefaultDeviceCodeIssuerPath}";

    }
}