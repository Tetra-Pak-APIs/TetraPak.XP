using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.Configuration;
using TetraPak.XP.Logging;

namespace TetraPak.XP.Auth.Abstractions
{
    // todo move to a more "web" oriented lib
    public static class TetraPakConfigurationHelper
    {
        static readonly object s_syncRoot = new();
        static bool s_isTetraPakConfigurationAdded;
        
        public static IServiceCollection AddTetraPakConfiguration(this IServiceCollection collection)
        {
            lock (s_syncRoot)
            {
                if (s_isTetraPakConfigurationAdded)
                    return collection;
                                
                s_isTetraPakConfigurationAdded = true;
            }

            Configure.InsertConfigurationDecorator(new FallbackConfigurationDecoratorDelegate());
            Configure.InsertValueDelegate(new TetraPakConfigurationValueDelegate());
            collection.AddSingleton<ITetraPakConfiguration>(p =>
            {
                var conf = p.GetRequiredService<IConfiguration>();
                var resolver = p.GetRequiredService<IRuntimeEnvironmentResolver>();
                var section = conf.GetSection(TetraPakConfiguration.SectionKey);
                var log = p.GetService<ILog>();
                var args = new ConfigurationSectionDecoratorArgs(null, conf, section, resolver, log);
                return new TetraPakConfiguration(args);
                
            });
            return collection;
        }
        
        class TetraPakConfigurationValueDelegate : IConfigurationValueDelegate
        {
            public bool IsFallbackDelegate => true;

            public Outcome<T> GetValue<T>(ConfigurationValueArgs<T> args)
            {
                if (args.Configuration is not TetraPakConfiguration tpConf)
                    return Outcome<T>.Fail(new Exception());

                if (args.Key != nameof(TetraPakConfiguration.RuntimeEnvironment))
                    return Outcome<T>.Fail(new Exception());
                
                var value = tpConf[nameof(TetraPakConfiguration.RuntimeEnvironment)];
                if (value.TryParseEnum(typeof(RuntimeEnvironment), out var env, true) && env is T tEnvironment)
                    return Outcome<T>.Success(tEnvironment);

                var resolver = tpConf.RuntimeEnvironmentResolver;
                env = resolver.ResolveRuntimeEnvironment(RuntimeEnvironment.Unknown);
                return env is T tv
                    ? Outcome<T>.Success(tv)
                    : Outcome<T>.Fail(new Exception("Cannot resolve runtime environment"));

            }
        }
        
        /// <summary>
        ///   Examines a string and returns a value to indicate whether the value identifies
        ///   an attribute used for auth configuration. This is to ensure there is no risk of confusing
        ///   services or endpoints with such attributes. 
        /// </summary>
        /// <param name="config">
        ///   The extended configuration object.
        /// </param>
        /// <param name="identifier">
        ///   The identifier being examined.
        /// </param>
        /// <returns>
        ///   <c>true</c> if <paramref name="identifier"/> matches an auth configuration attribute; otherwise <c>false</c>. 
        /// </returns>
        /// <remarks>
        ///   Examples of auth identifiers: "<c>ConfigPath</c>", "<c>GrantType</c>",
        ///   "<c>ClientId</c>", "<c>ClientSecret</c>", "<c>Scope</c>".
        /// </remarks>
        // ReSharper disable once UnusedParameter.Global
        public static bool IsAuthIdentifier(this IConfiguration config, string identifier)
        {
            return identifier switch
            {
                nameof(IAuthConfiguration.AuthorityUri) => true,
                nameof(IAuthConfiguration.TokenIssuerUri) => true,
                nameof(IAuthConfiguration.DeviceCodeIssuerUri) => true,
                nameof(IAuthConfiguration.RedirectUri) => true,
                nameof(IAuthConfiguration.GrantType) => true,
                nameof(IAuthConfiguration.ClientId) => true,
                nameof(IAuthConfiguration.ClientSecret) => true,
                nameof(IAuthConfiguration.OidcScope) => true,
                _ => false
            };
        }
        
        /// <summary>
        ///   Constructs and returns an <see cref="Outcome{T}"/> to reflect a required configuration item is missing. 
        /// </summary>
        public static Outcome<T> MissingConfigurationOutcome<T>(this IConfigurationSection cfg, string key) 
            => 
                Outcome<T>.Fail(MissingConfigurationException(cfg, key));
         
        /// <summary>
        ///   Constructs and returns an <see cref="Exception"/> to reflect a required configuration item is missing. 
        /// </summary>
        public static Exception MissingConfigurationException(this IConfigurationSection cfg, string key)
            =>
                new ConfigurationException($"Missing configuration: {new ConfigPath(cfg.Path).Push(key)}");
        
        /// <summary>
        ///   Constructs and returns an <see cref="Outcome{T}"/> to reflect a configuration item is invalid. 
        /// </summary>
        public static Outcome<T> InvalidConfigurationOutcome<T>(this IConfigurationSection cfg, string key, object value) 
            => 
                Outcome<T>.Fail(InvalidConfigurationException(cfg, key, value));

        /// <summary>
        ///   Constructs and returns an <see cref="Exception"/> to reflect a configuration item is invalid. 
        /// </summary>
        public static Exception InvalidConfigurationException(this IConfigurationSection cfg, string key, object value) 
            => 
                new ConfigurationException($"Invalid configuration: {new ConfigPath(cfg.Path).Push(key)}: {value}");

    }
    
    class FallbackConfigurationDecoratorDelegate : IConfigurationDecoratorDelegate
    {
        public bool IsFallbackDecorator => true;
        public Outcome<ConfigurationSectionDecorator> WrapSection(ConfigurationSectionDecoratorArgs args)
        {
            throw new NotImplementedException();
        }
    }
}