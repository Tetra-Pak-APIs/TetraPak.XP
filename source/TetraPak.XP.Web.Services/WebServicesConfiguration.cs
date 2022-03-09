using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Configuration;

namespace TetraPak.XP.Web.Services
{
    // todo move to a more "web" oriented lib
    public class WebServicesConfiguration : ConfigurationSectionWrapper, IWebServicesConfiguration
    {
        internal const string SectionKey = "WebServices";
        static readonly ConfigPath s_webServicesPath = new(new[] { TetraPakConfiguration.SectionKey, SectionKey });

        public static IWebServicesConfiguration Empty => new WebServicesConfiguration();
        
        public IWebServiceConfiguration? GetConfiguration(string serviceName)
        {
            var section = GetSection(serviceName);
            return section is IWebServiceConfiguration wsConf ? wsConf : null;
        }
        
        internal static void InsertWrapperDelegates()
        {
            Configure.InsertConfigurationDecorator(new WebServicesConfigurationDecoratorDelegate());
        }

        protected override ConfigurationSectionWrapper[] OnBuildWrapperGraph(IConfigurationSection rootSection)
        {
            var serviceSections = Section!.GetSubSections();
            var wrappedSections = new List<ConfigurationSectionWrapper>();
            foreach (var serviceSection in serviceSections)
            {
                var args = CreateSectionWrapperArgs(serviceSection, this);
                var wsConf = new TetraPakWebServiceConfiguration(args);
                wrappedSections.Add(wsConf);
            }
            return wrappedSections.ToArray();
        }

        static ConfigurationSectionWrapper? getWebServiceCollectionConfiguration(
            ConfigurationSectionDecoratorArgs args)
        {
            return args.Section.Path == s_webServicesPath 
                ? new WebServicesConfiguration(args) 
                : null;
        }

        WebServicesConfiguration()
        {
        }
        
        public WebServicesConfiguration(ConfigurationSectionDecoratorArgs args)
        : base(args)
        {
            this.SetAsSingletonService();
        }
        
        class WebServicesConfigurationDecoratorDelegate : IConfigurationDecoratorDelegate
        {
            public bool IsFallbackDecorator => false;

            public Outcome<ConfigurationSectionWrapper> WrapSection(ConfigurationSectionDecoratorArgs args)
            {
                return args.Section.Path == s_webServicesPath 
                    ? Outcome<ConfigurationSectionWrapper>.Success(new WebServicesConfiguration(args)) 
                    : Outcome<ConfigurationSectionWrapper>.Fail("");
            }
        }
    }
}