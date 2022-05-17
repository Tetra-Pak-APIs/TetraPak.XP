using System.Collections.Generic;
using System.Linq;
using TetraPak.XP.Auth.Abstractions;

namespace TetraPak.XP.Desktop
{
    public static class DesktopHostAuthorizationOptionsHelper
    {
        public static GrantType[] DefaultGrantTypes { get; } = { GrantType.AC };
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
        public static HostAuthorizationOptions ForDefaultTetraPakDesktopApp(this HostAuthorizationOptions options)
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

        public static HostAuthorizationOptions WithClientCredentialsGrant(this HostAuthorizationOptions options)
        {
            if (!options.GrantTypes.Contains(GrantType.CC))
            {
                options.GrantTypes = new List<GrantType>(options.GrantTypes) { GrantType.CC }.ToArray();
            }
            return options;
        }

        public static HostAuthorizationOptions WithTokenExchangeGrant(this HostAuthorizationOptions options)
        {
            if (!options.GrantTypes.Contains(GrantType.TX))
            {
                options.GrantTypes = new List<GrantType>(options.GrantTypes) { GrantType.TX }.ToArray();
            }
            return options;
        }

    }
}