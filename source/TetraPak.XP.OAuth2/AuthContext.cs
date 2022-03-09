using System;
using System.Threading;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Configuration;
using TetraPak.XP.Web.Services;

namespace TetraPak.XP.OAuth2
{
    /// <summary>
    ///   Used to describe an auth request context.
    /// </summary>
    public class AuthContext
    {
        /// <summary>
        ///   Gets the <see cref="IAuthConfiguration"/> object.
        /// </summary>
        public IAuthConfiguration Configuration { get; }

        public GrantOptions Options { get; }
        
        public GrantType GrantType { get; }

        public CancellationToken CancellationToken => Options.CancellationTokenSource?.Token ?? CancellationToken.None;

        /// <summary>
        ///   Initializes the <see cref="AuthContext"/>.
        /// </summary>
        /// <param name="grantType">
        ///   Initializes <see cref="GrantType"/>.
        /// </param>
        /// <param name="configuration">
        ///   Initializes <see cref="Configuration"/>. 
        /// </param>
        /// <param name="options">
        ///   Specifies options for the ongoing <see cref="Grant"/> request.
        /// </param>
        internal AuthContext(GrantType grantType, IAuthConfiguration configuration, GrantOptions options)
        {
            GrantType = grantType;
            Options = options;
            Configuration = configuration;
        }
    }
    
    public static class AuthContextHelper
    {
        /// <summary>
        ///   Constructs and returns a <see cref="AuthContext"/>. 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="grantType">
        ///   Specifies the requested <see cref="GrantType"/>.
        /// </param>
        /// <param name="options">
        ///   Options describing the request.
        /// </param>
        /// <returns>
        ///   
        /// </returns>
        public static Outcome<AuthContext> GetAuthContext(
            this ITetraPakConfiguration self, 
            GrantType grantType, 
            GrantOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.Service))
                return Outcome<AuthContext>.Success(new AuthContext(grantType, self, options));

            var servicesSection = self.GetSubSection(ConfigurationSectionNames.Services);
            if (servicesSection is not IWebServicesConfiguration webServices)
                return Outcome<AuthContext>.Fail(new Exception("No web services was configured"));

            var section = webServices.GetSubSection(options.Service);
            return section is not IWebServiceConfiguration wsSection
                ? AuthConfiguration.MissingConfigurationOutcome<AuthContext>(servicesSection, options.Service) 
                : Outcome<AuthContext>.Success(new AuthContext(grantType, wsSection, options));
        }
    }
}