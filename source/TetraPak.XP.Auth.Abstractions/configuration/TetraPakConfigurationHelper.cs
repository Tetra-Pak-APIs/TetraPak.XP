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
        
        public static IServiceCollection UseTetraPakConfiguration(this IServiceCollection collection)
        {
            lock (s_syncRoot)
            {
                if (s_isTetraPakConfigurationAdded)
                    return collection;
                                
                s_isTetraPakConfigurationAdded = true;
            }

            Configure.InsertWrapperDelegate(e => new ConfigurationSectionWrapper(e));
            Configure.InsertValueDelegate(new TetraPakConfigurationValueDelegate());
            collection.AddSingleton<ITetraPakConfiguration>(p =>
            {
                var conf = p.GetRequiredService<IConfiguration>();
                var resolver = p.GetRequiredService<IRuntimeEnvironmentResolver>();
                var section = conf.GetSection(TetraPakConfiguration.SectionKey);
                var log = p.GetService<ILog>();
                var args = new ConfigurationSectionWrapperArgs(null, conf, section, resolver, log);
                return new TetraPakConfiguration(args);
                
            });
            return collection;
        }
        
        class TetraPakConfigurationValueDelegate : IConfigurationValueDelegate
        {
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
                env = resolver.ResolveRuntimeEnvironment();
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
    }
}