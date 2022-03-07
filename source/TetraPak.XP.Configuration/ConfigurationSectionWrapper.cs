using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using TetraPak.XP.Logging;

namespace TetraPak.XP.Configuration
{
    public class ConfigurationSectionWrapper : IConfigurationSection
    {
        protected readonly IConfigurationSection? Section;
        
        readonly Dictionary<string, ConfigurationSectionWrapper>? _childSections;

        protected IConfiguration _configuration { get; }

        protected IRuntimeEnvironmentResolver _runtimeEnvironmentResolver { get; }

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
            
            sectionWrapper = new ConfigurationSectionWrapper(CreateSectionWrapperArgs(section, this));
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

        protected virtual ConfigurationSectionWrapper[] OnBuildWrapperGraph(IConfigurationSection rootSection)
        {
            var children = rootSection.GetSubSections();
            var childWrappers = new List<ConfigurationSectionWrapper>();
            var wrapperDelegates = Configure.GetSectionWrapperDelegates();
            foreach (var childSection in children)
            {
                childWrappers.Add(OnWrapConfigurationSection(childSection, this, wrapperDelegates));
            }

            return childWrappers.ToArray();
        }

        protected ConfigurationSectionWrapperArgs CreateSectionWrapperArgs(
            IConfigurationSection section,
            ConfigurationSectionWrapper parent)
            => new(
                parent,
                _configuration,
                section,
                _runtimeEnvironmentResolver,
                Log);
            
        protected virtual ConfigurationSectionWrapper OnWrapConfigurationSection(
            IConfigurationSection section, 
            ConfigurationSectionWrapper parent,
            ConfigurationSectionWrapperDelegate[] wrapperDelegates)
        {
            var args = CreateSectionWrapperArgs(section, parent);
            for (var i = 0; i < wrapperDelegates.Length; i++)
            {
                var wrapperDelegate = wrapperDelegates[i];
                var wrapper = wrapperDelegate(args);
                if (wrapper is null) 
                    continue;
                
                wrapper.Parent = parent;
                return wrapper;
            }
            
            return new ConfigurationSectionWrapper(args)
            {
                Parent = parent
            };
        }

        ConfigurationSectionWrapper[] buildWrapperGraph(IConfigurationSection rootSection)
        {
            var sections = OnBuildWrapperGraph(rootSection);
            for (var i = 0; i < sections.Length; i++)
            {
                var section = sections[i];
                section.Parent ??= this;
            }

            return sections;
        }

        protected ConfigurationSectionWrapper()
        {
            _configuration = null!;
            _runtimeEnvironmentResolver = null!;
            Section = null!;
        }
        
        public ConfigurationSectionWrapper(ConfigurationSectionWrapperArgs args)
        {
            _configuration = args.Configuration;
            Log = args.Log;
            Section = args.Section;
            _runtimeEnvironmentResolver = args.RuntimeEnvironmentResolver;
            _childSections = buildWrapperGraph(this).ToDictionary(i => i.Key);
        }
    }
}