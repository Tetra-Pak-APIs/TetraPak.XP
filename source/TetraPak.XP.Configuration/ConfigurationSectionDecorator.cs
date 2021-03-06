using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using TetraPak.XP.Logging.Abstractions;

namespace TetraPak.XP.Configuration
{
    public class ConfigurationSectionDecorator : IConfigurationSection
    {
        readonly ConfigurationSectionDecoratorArgs? _args;

        internal Dictionary<string,object?>? Overrides { get; set; }

        protected IConfigurationSection? Section => _args?.Section;

        internal IConfigurationSection? GetSection() => Section;

        readonly Dictionary<string, ConfigurationSectionDecorator>? _childSections;

        protected IConfiguration? Configuration => _args?.Configuration; 

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

        public ILog? Log => _args?.Log;

        internal IConfigurationSection? Parent { get; private set; }

        public ConfigurationSectionDecorator Clone<T>(bool includeOverrides) where T : ConfigurationSectionDecorator
        {
            if (_args is null)
                throw new InvalidOperationException($"Cannot clone uninitialized {typeof(ConfigurationSectionDecorator)}");

            var cloned = (T)Activator.CreateInstance(typeof(T), _args)!;
            cloned.Parent = Parent;
            
            if (includeOverrides && Overrides is {})
            {
                cloned.Overrides = new Dictionary<string, object?>(Overrides);
            }

            return cloned;
        }
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

        public IDictionary<string, object?> GetOverrides() => Overrides ?? new Dictionary<string, object?>();

        public IEnumerable<IConfigurationSection> GetChildren() => IsEmpty
            ? Array.Empty<IConfigurationSection>()
            : _childSections?.Values.Any() ?? false 
                ? _childSections!.Values 
                : Section?.GetChildren() ?? Array.Empty<IConfigurationSection>();

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
                Configuration!,
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
            _args = null!;
            _runtimeEnvironmentResolver = null!;
            // Section = null!; obsolete
        }

        public ConfigurationSectionDecorator(ConfigurationSectionDecoratorArgs args)
        {
            _args = args;
            // _configuration = args.Configuration; obsolete
            // Log = args.Log;
            // Section = args.Section;
            _runtimeEnvironmentResolver = args.RuntimeEnvironmentResolver;
            _childSections = buildWrapperGraph(this).ToDictionary(i => i.Key);
        }
    }
}