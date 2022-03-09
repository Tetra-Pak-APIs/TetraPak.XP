using TetraPak.XP.Configuration;
using TetraPak.XP.Web.Abstractions;

namespace TetraPak.XP.Auth.Abstractions
{
    class TetraPakConfiguration : AuthConfiguration, ITetraPakConfiguration
    {
        public const string SectionKey = "TetraPak";
        RuntimeEnvironment? _runtimeEnvironment; 

        public string? MessageIdHeader => this.Get(getDerived:true, useDefault: Headers.MessageId);
        
        public override RuntimeEnvironment RuntimeEnvironment
        {
            get
            {
                _runtimeEnvironment ??= RuntimeEnvironmentResolver.ResolveRuntimeEnvironment();
                return _runtimeEnvironment.Value;
            }
        }

        public TetraPakConfiguration(ConfigurationSectionDecoratorArgs args)
        : base(args)
        {
        }
    }
}