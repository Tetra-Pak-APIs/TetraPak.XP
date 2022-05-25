using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.ApplicationInformation;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Identity;
using TetraPak.XP.OAuth2.DeviceCode;
using TetraPak.XP.OAuth2.OIDC;
using Xamarin.Forms;

namespace TetraPak.XP.Mobile
{
    /// <summary>
    ///   Provides convenient helper methods for configuring a mobile Tetra Pak application. 
    /// </summary>
    public static class MobileServicesHelper
    {
        /// <summary>
        ///   Just a convenient (fluent api) method to automatically register all services
        ///   of the "Mobile" package. 
        /// </summary>
        public static XpServicesBuilder Mobile(this XpPlatformServicesBuilder builder)
        {
            ApplicationInfo.Current = new ApplicationInfo(
                ApplicationType.Mobile, 
                ApplicationFramework.Xamarin,
                Device.RuntimePlatform,
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
        ///   Specifies options for the mobile authorization (see <see cref="MobileHostAuthorizationOptionsHelper.ForDefaultTetraPakMobileApp"/>).
        /// </param>
        /// <returns>
        ///   The service <paramref name="collection"/>.
        /// </returns>
        public static IServiceCollection AddTetraPakXamarinAuthorization(
            this IServiceCollection collection, 
            HostAuthorizationOptions? options = null)
        {
            options ??= new HostAuthorizationOptions().ForDefaultTetraPakMobileApp();
            if (options.GrantTypes.Contains(GrantType.OIDC) || options.GrantTypes.Contains(GrantType.AC))
            {
                collection.AddTetraPakOidcGrant();
            }

            if (options.GrantTypes.Contains(GrantType.DC))
            {
                collection.AddTetraPakDeviceCodeGrant();
            }

            if (options.IsGrantCacheSupported)
            {
                collection.AddMobileTokenCache();
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
        ///   (optional; default=<see cref="MobileHostAuthorizationOptionsHelper.DefaultIsGrantCacheSupported"/>)<br/>
        ///   Specifies whether to support grant caching (and thus "silent" grant acquisition).
        /// </param>
        /// <param name="isUserInformationSupported">
        ///   (optional; default=<see cref="MobileHostAuthorizationOptionsHelper.DefaultIsUserInformationSupported"/>)<br/>
        ///   Specifies whether to support fetching user information, based on acquired grants.
        /// </param>
        /// <param name="grantTypes">
        ///   Specifies the required grant types.
        ///   Please note that grant types not supported by a mobile client will be ignored.
        /// </param>
        /// <returns>
        ///   The service <paramref name="collection"/>.
        /// </returns>
        public static IServiceCollection AddTetraPakXamarinAuthorization(
            this IServiceCollection collection,
            bool? isGrantCachingSupported = null,
            bool? isUserInformationSupported = null,
            params GrantType[] grantTypes)
        {
            grantTypes = grantTypes.Any() ? grantTypes : MobileHostAuthorizationOptionsHelper.DefaultGrantTypes;
            return collection.AddTetraPakXamarinAuthorization(new HostAuthorizationOptions(grantTypes)
            {
                IsGrantCacheSupported = isGrantCachingSupported ?? MobileHostAuthorizationOptionsHelper.DefaultIsGrantCacheSupported,
                IsUserInformationSupported = isUserInformationSupported ?? MobileHostAuthorizationOptionsHelper.DefaultIsUserInformationSupported
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
        public static IServiceCollection AddTetraPakXamarinAuthorization(
            this IServiceCollection collection,
            params GrantType[] grantTypes)
        {
            return collection.AddTetraPakXamarinAuthorization(
                MobileHostAuthorizationOptionsHelper.DefaultIsGrantCacheSupported,
                MobileHostAuthorizationOptionsHelper.DefaultIsUserInformationSupported,
                grantTypes);
        }
    }
}