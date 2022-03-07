using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Configuration;
using TetraPak.XP.Logging;

namespace TetraPak.XP.Web.Services
{
    // todo move to a more "web" oriented lib

    class TetraPakWebServiceConfiguration : AuthConfiguration, IWebServiceConfiguration
    {
        /// <inheritdoc />
        [StateDump]
        public string BaseAddress 
            => this.Get<string>(getDerived: true) 
               ?? throw new ConfigurationException(
                   $"Web service base path is not configured: {new ConfigPath(Path)}");

        /// <inheritdoc />
        [StateDump]
        public string ServicePath => this.Get<string>() ?? string.Empty;

        public TetraPakWebServiceConfiguration(ConfigurationSectionWrapperArgs args) 
        : base(args)
        {
        }
    }
}