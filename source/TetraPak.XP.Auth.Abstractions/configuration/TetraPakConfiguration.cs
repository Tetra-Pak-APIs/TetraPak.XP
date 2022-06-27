using TetraPak.XP.Configuration;
using TetraPak.XP.Web.Abstractions;

namespace TetraPak.XP.Auth.Abstractions
{
    public class TetraPakConfiguration : AuthConfiguration, ITetraPakConfiguration
    {
        public const string SectionKey = "TetraPak";

        public string? MessageIdHeader => this.Get(useDefault: Headers.MessageId, getDerived: true);
        
        public override RuntimeEnvironment RuntimeEnvironment
        {
            get
            {
                var configured = base.RuntimeEnvironment;
                return configured != RuntimeEnvironment.Unknown 
                    ? configured 
                    : RuntimeEnvironment.Production;
            }
        }

        public TetraPakConfiguration(ConfigurationSectionDecoratorArgs args)
        : base(args)
        {
        }
    }
}