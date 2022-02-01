using TetraPak.XP.Configuration;
using TetraPak.XP.Logging;

namespace TetraPak.XP.Auth.Abstractions
{
    public class TetraPakConfig : ConfigurationSection, IServiceAuthConfig
    {
        /// <inheritdoc />
        [StateDump]
        public virtual GrantType GrantType
        {
            get => GetFromFieldThenSection(GrantType.None, 
                (string? value, out GrantType grantType) =>
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        grantType = GrantType.None;
                        return true;
                    }
                    
                    if (!TryParseEnum(value, out grantType) || grantType == GrantType.Inherited)
                        throw new HttpServerConfigurationException($"Invalid auth method: '{value}' ({DefaultSectionIdentifier}.{nameof(GrantType)})");

                    return true;
                });
            set => _method = value;
        }
    }
}