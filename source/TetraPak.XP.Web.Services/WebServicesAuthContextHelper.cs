using System;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Configuration;

namespace TetraPak.XP.Web.Services
{
    public static class WebServicesAuthContextHelper
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

            var section = webServices.GetSubSection(options.Service!);
            return section is not IWebServiceConfiguration wsSection
                ? servicesSection.MissingConfigurationOutcome<AuthContext>(options.Service!) 
                : Outcome<AuthContext>.Success(new AuthContext(grantType, wsSection, options));
        }
    }
}