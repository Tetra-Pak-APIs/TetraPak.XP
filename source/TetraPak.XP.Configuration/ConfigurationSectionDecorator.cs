using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using TetraPak.XP.Logging;

namespace TetraPak.XP.Configuration
{
    public class ConfigurationSectionDecorator : IConfigurationSection
    {
        protected IConfigurationSection? Section { get; }

        internal IConfigurationSection? GetSection() => Section;
        
        readonly Dictionary<string, ConfigurationSectionDecorator>? _childSections;

        readonly IConfiguration _configuration;

        readonly IRuntimeEnvironmentResolver _runtimeEnvironmentResolver;
        
        public bool IsEmpty => Section is null;
        
        public string Key => Section?.Key ?? string.Empty;

        public string Path => Section?.Path ?? string.Empty;
       
        public string Value
        {
            get => Section?.Value ?? string.Empty;
            set
            {
                if (Section is null)
                    return;
                        
                Section.Value = value;
            }
        }

        public ILog? Log { get; }

        internal IConfigurationSection? Parent { get; set; }
        
        public IConfigurationSection? GetSection(string key)
        {
            if (IsEmpty)
                return null!;
            
            if (_childSections is null)
                return Section!.GetSection(key);

            if (_childSections.TryGetValue(key, out var sectionWrapper))
                return sectionWrapper;

            var section = Section!.GetSection(key);
            if (!section.IsConfigurationSection())
                return section;
            
            sectionWrapper = new ConfigurationSectionDecorator(CreateSectionWrapperArgs(section, this));
            _childSections.Add(key, sectionWrapper);
            return sectionWrapper;
        }

        public IEnumerable<IConfigurationSection> GetChildren() => IsEmpty 
            ? Array.Empty<IConfigurationSection>() 
            : _childSections?.Values ?? Section?.GetChildren() ?? Array.Empty<IConfigurationSection>();

        public IChangeToken GetReloadToken() => Section?.GetReloadToken() ?? null!;

        public string this[string key]
        {
            get => IsEmpty ? string.Empty : Section![key] ?? string.Empty;
            set
            {
                if (IsEmpty)
                    return;
                
                Section![key] = value;
            }
        }

        protected virtual ConfigurationSectionDecorator[] OnBuildWrapperGraph(IConfigurationSection rootSection)
        {
            var children = rootSection.GetSubSections();
            var childWrappers = new List<ConfigurationSectionDecorator>();
            var wrapperDelegates = Configure.GetConfigurationDecorators();
            foreach (var childSection in children)
            {
                childWrappers.Add(OnWrapConfigurationSection(childSection, this, wrapperDelegates));
            }

            return childWrappers.ToArray();
        }

        protected ConfigurationSectionDecoratorArgs CreateSectionWrapperArgs(
            IConfigurationSection section,
            ConfigurationSectionDecorator parent)
            => new(
                parent,
                _configuration,
                section,
                _runtimeEnvironmentResolver,
                Log);
            
        protected virtual ConfigurationSectionDecorator OnWrapConfigurationSection(
            IConfigurationSection section, 
            ConfigurationSectionDecorator parent,
            IConfigurationDecoratorDelegate[] decorators)
        {
            var args = CreateSectionWrapperArgs(section, parent);
            for (var i = 0; i < decorators.Length; i++)
            {
                var decorator = decorators[i];
                var outcome = decorator.WrapSection(args);
                if (!outcome)
                    continue;

                var wrapper = outcome.Value!;
                wrapper.Parent = parent;
                return wrapper;
            }
            
            return new ConfigurationSectionDecorator(args)
            {
                Parent = parent
            };
        }

        ConfigurationSectionDecorator[] buildWrapperGraph(IConfigurationSection rootSection)
        {
            var sections = OnBuildWrapperGraph(rootSection);
            for (var i = 0; i < sections.Length; i++)
            {
                var section = sections[i];
                section.Parent ??= this;
            }

            return sections;
        }

        protected ConfigurationSectionDecorator()
        {
            _configuration = null!;
            _runtimeEnvironmentResolver = null!;
            Section = null!;
        }
        
        public ConfigurationSectionDecorator(ConfigurationSectionDecoratorArgs args)
        {
            _configuration = args.Configuration;
            Log = args.Log;
            Section = args.Section;
            _runtimeEnvironmentResolver = args.RuntimeEnvironmentResolver;
            _childSections = buildWrapperGraph(this).ToDictionary(i => i.Key);
        }
    }
}