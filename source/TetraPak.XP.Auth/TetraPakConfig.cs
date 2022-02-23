using System.Threading.Tasks;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Caching.Abstractions;
using TetraPak.XP.Configuration;
using TetraPak.XP.Logging;

namespace TetraPak.XP.Auth
{
    public class TetraPakConfig : ServiceAuthConfigSectionWrapper, ITetraPakConfiguration
    {
        const string SectionKey = "TetraPak";
        readonly ITimeLimitedRepositories? _cache;

        public string? RequestMessageIdHeader => Section?.Get<string?>();

        /// <inheritdoc />
        public async Task<Outcome<AuthContext>> GetAuthContextAsync(GrantType grantType, GrantOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.Service))
                return Outcome<AuthContext>.Success(new AuthContext(grantType, this, options));

            var path = new ConfigPath(options.Service);
            if (path.Count < 2)
            {
                path = path.Insert(ConfigurationSectionNames.Services);
            }

            return await GetSectionAsync(path) is not IServiceAuthConfig section 
                ? Outcome<AuthContext>.Fail(new ConfigurationException($"Could not find configured service \"{path}\"")) 
                : Outcome<AuthContext>.Success(new AuthContext(grantType, section, options));
        }

        public TetraPakConfig(
            IConfigurationSectionExtended section, 
            IRuntimeEnvironmentResolver environmentResolver,
            ITimeLimitedRepositories? cache = null,
            ITokenCache? tokenCache = null,
            ILog? log = null) 
        : base(section, environmentResolver, tokenCache, log)
        {
            _cache = cache;
        }

        public TetraPakConfig(
            IConfiguration? configuration,
            IRuntimeEnvironmentResolver environmentResolver,
            ITimeLimitedRepositories? cache = null,
            ITokenCache? tokenCache = null,
            ILog? log = null) 
        : base(configuration, SectionKey, environmentResolver, tokenCache, log)
        {
            _cache = cache;
        }
    }

}