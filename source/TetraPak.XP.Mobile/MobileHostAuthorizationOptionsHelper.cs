using System.Collections.Generic;
using System.Linq;
using TetraPak.XP.Auth.Abstractions;

namespace TetraPak.XP.Mobile
{
    /// <summary>
    ///   Provides convenient helper method for setting up authorization for a Tetra Pak mobile application.
    /// </summary>
    public static class MobileHostAuthorizationOptionsHelper
    {
        public static GrantType[] DefaultGrantTypes { get; } = { GrantType.DC };
        public const bool DefaultIsGrantCacheSupported = true;
        public const bool DefaultIsUserInformationSupported = true;
        
        /// <summary>
        ///   Specifies default options for a default Tetra Pak mobile application. 
        /// </summary>
        /// <param name="options">
        ///   The <see cref="HostAuthorizationOptions"/>
        /// </param>
        /// <returns>
        /// </returns>
        public static HostAuthorizationOptions ForDefaultTetraPakMobileApp(this HostAuthorizationOptions options)
        {
            options.GrantTypes = new[] { GrantType.AuthorizationCode };
            options.IsGrantCacheSupported = DefaultIsGrantCacheSupported;
            options.IsUserInformationSupported = DefaultIsUserInformationSupported;
            return options;
        }
        
        public static HostAuthorizationOptions WithGrantTypes(
            this HostAuthorizationOptions options, 
            params GrantType[] grantTypes)
        {
            options.GrantTypes = grantTypes.Any() ? grantTypes : DefaultGrantTypes;
            return options;
        }

        public static HostAuthorizationOptions WithDeviceCodeGrant(this HostAuthorizationOptions options)
        {
            if (!options.GrantTypes.Contains(GrantType.DC))
            {
                options.GrantTypes = new List<GrantType>(options.GrantTypes) { GrantType.DC }.ToArray();
            }
            return options;
        }

    }
}