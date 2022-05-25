using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.ApplicationInformation;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Identity;
using TetraPak.XP.OAuth2.ClientCredentials;
using TetraPak.XP.OAuth2.DeviceCode;
using TetraPak.XP.OAuth2.OIDC;
using TetraPak.XP.OAuth2.TokenExchange;
using TetraPak.XP.Web.Http;

namespace TetraPak.XP.Desktop
{
    public static class DesktopServicesHelper
    {
        
        /// <summary>
        ///   (fluent api)<br/>
        ///   Prepares a desktop host for a specified framework and returns a <see cref="XpServicesBuilder"/>. 
        /// </summary>
        /// <seealso cref="DesktopCustom"/>
        public static XpServicesBuilder Desktop(this XpPlatformServicesBuilder builder, ApplicationFramework framework)
        {
            ApplicationInfo.Current = new ApplicationInfo(
                ApplicationType.Desktop, 
                framework, 
                Environment.OSVersion.ToString(), 
                 SdkHelper.NugetPackageVersion);
            return builder.Build();
        }
        
        /// <summary>
        ///   (fluent api)<br/>
        ///   Prepares a desktop host with a custom framework and returns a <see cref="XpServicesBuilder"/>. 
        /// </summary>
        /// <seealso cref="DesktopCustom"/>
        public static XpServicesBuilder DesktopCustom(this XpPlatformServicesBuilder builder, string framework)
        {
            ApplicationInfo.Current = new ApplicationInfo(
                ApplicationType.Desktop, 
                framework, 
                Environment.OSVersion.ToString(), 
                SdkHelper.NugetPackageVersion);
            return builder.Build();
        }

        /// <summary>
        ///   Configures a mobile app for default Tetra Pak mobile authorization. 
        /// </summary>
        /// <param name="collection">
        ///   The <see cref="IServiceCollection"/> to be configured.
        /// </param>
        /// <param name="options">
        ///   (optional; default="typical Tetra Pak auth options")<br/>
        ///   Specifies options for the mobile authorization (see <see cref="DesktopHostAuthorizationOptionsHelper.ForDefaultTetraPakDesktopApp"/>).
        /// </param>
        /// <returns>
        ///   The service <paramref name="collection"/>.
        /// </returns>
        public static IServiceCollection AddTetraPakDesktopAuthorization(
            this IServiceCollection collection, 
            HostAuthorizationOptions? options = null)
        {
            options ??= new HostAuthorizationOptions().ForDefaultTetraPakDesktopApp();
            if (options.GrantTypes.Contains(GrantType.OIDC) || options.GrantTypes.Contains(GrantType.AC))
            {
                collection.AddTetraPakOidcGrant();
            }

            if (options.GrantTypes.Contains(GrantType.AC) || options.GrantTypes.Contains(GrantType.OIDC))
            {
                collection.AddTetraPakOidcGrant();
            }

            if (options.GrantTypes.Contains(GrantType.DC))
            {
                collection.AddTetraPakDeviceCodeGrant();
            }

            if (options.GrantTypes.Contains(GrantType.CC))
            {
                collection.AddTetraPakClientCredentialsGrant();
            }

            if (options.GrantTypes.Contains(GrantType.TX))
            {
                collection.AddTetraPakTokenExchangeGrant();
            }

            if (options.IsGrantCacheSupported)
            {
                collection.AddDesktopTokenCache();
            }

            if (options.IsUserInformationSupported)
            {
                collection.AddTetraPakUserInformation();
            }

            return collection;
        }
        
        /// <summary>
        ///   Configures a mobile app for Tetra Pak authorization while specifying
        ///   required authorization services. 
        /// </summary>
        /// <param name="collection">
        ///   The <see cref="IServiceCollection"/> to be configured.
        /// </param>
        /// <param name="isGrantCachingSupported">
        ///   (optional; default=<see cref="DesktopHostAuthorizationOptionsHelper.DefaultIsGrantCacheSupported"/>)<br/>
        ///   Specifies whether to support grant caching (and thus "silent" grant acquisition).
        /// </param>
        /// <param name="isUserInformationSupported">
        ///   (optional; default=<see cref="DesktopHostAuthorizationOptionsHelper.DefaultIsUserInformationSupported"/>)<br/>
        ///   Specifies whether to support fetching user information, based on acquired grants.
        /// </param>
        /// <param name="grantTypes">
        ///   Specifies the required grant types.
        ///   Please note that grant types not supported by a mobile client will be ignored.
        /// </param>
        /// <returns>
        ///   The service <paramref name="collection"/>.
        /// </returns>
        public static IServiceCollection AddTetraPakDesktopAuthorization(
            this IServiceCollection collection,
            bool? isGrantCachingSupported = null,
            bool? isUserInformationSupported = null,
            params GrantType[] grantTypes)
        {
            grantTypes = grantTypes.Any() ? grantTypes : DesktopHostAuthorizationOptionsHelper.DefaultGrantTypes;
            return collection.AddTetraPakDesktopAuthorization(new HostAuthorizationOptions(grantTypes)
            {
                IsGrantCacheSupported = isGrantCachingSupported ?? DesktopHostAuthorizationOptionsHelper.DefaultIsGrantCacheSupported,
                IsUserInformationSupported = isUserInformationSupported ?? DesktopHostAuthorizationOptionsHelper.DefaultIsUserInformationSupported
            });
        }
                
        /// <summary>
        ///   Configures a mobile app for Tetra Pak mobile authorization while specifying
        ///   required authorization services. 
        /// </summary>
        /// <param name="collection">
        ///   The <see cref="IServiceCollection"/> to be configured.
        /// </param>
        /// <param name="grantTypes">
        ///   Specifies the required grant types.
        ///   Please note that grant types not supported by a mobile client will be ignored.
        /// </param>
        /// <returns>
        ///   The service <paramref name="collection"/>.
        /// </returns>
        public static IServiceCollection AddTetraPakDesktopAuthorization(
            this IServiceCollection collection,
            params GrantType[] grantTypes)
        {
            return collection.AddTetraPakDesktopAuthorization(
                DesktopHostAuthorizationOptionsHelper.DefaultIsGrantCacheSupported,
                DesktopHostAuthorizationOptionsHelper.DefaultIsUserInformationSupported,
                grantTypes);
        }
    }
}