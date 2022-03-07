﻿// using System.Threading.Tasks;
// using TetraPak.XP.Auth.Abstractions;
// using TetraPak.XP.Caching.Abstractions; obsolete
// using TetraPak.XP.Configuration; 
// using TetraPak.XP.Logging;
//
// namespace TetraPak.XP.Auth
// {
//     public class TetraPakConfig : ServiceAuthConfigSectionWrapper, ITetraPakConfiguration
//     {
//         const string SectionKey = "TetraPak";
//         readonly ITimeLimitedRepositories? _cache;
//         readonly IRuntimeEnvironmentResolver _environmentResolver;
//
//         public string? RequestMessageIdHeader => Section?.Get<string?>();
//         
//         /// <summary>
//         ///   Gets the current runtime environment (DEV, TEST, PROD ...).
//         ///   The value is a <see cref="RuntimeEnvironment"/> enum value. 
//         /// </summary>
//         [StateDump]
//         public RuntimeEnvironment Environment
//         {
//             get
//             {
//                 var resolved = _environmentResolver.ResolveRuntimeEnvironment();
//                 return resolved == RuntimeEnvironment.Unknown
//                     ? Get<RuntimeEnvironment?>() ?? RuntimeEnvironment.Production
//                     : resolved;
//             }
//         } 
//
//         public TetraPakConfig(
//             IConfigurationSection section, 
//             IRuntimeEnvironmentResolver environmentResolver,
//             ITimeLimitedRepositories? cache = null,
//             ITokenCache? tokenCache = null,
//             ILog? log = null) 
//         : base(section, tokenCache, log)
//         {
//             _environmentResolver = environmentResolver;
//             _cache = cache;
//         }
//
//         // public TetraPakConfig(
//         //     IConfiguration? configuration,
//         //     IRuntimeEnvironmentResolver environmentResolver,
//         //     ITimeLimitedRepositories? cache = null,
//         //     ITokenCache? tokenCache = null,
//         //     ILog? log = null) 
//         // : base(configuration, SectionKey, tokenCache, log)
//         // {
//         //     _environmentResolver = environmentResolver;
//         //     _cache = cache;
//         // }
//     }
//
//     public static class TetraPakConfigurationHelper
//     {
//         /// <summary>
//         ///   Constructs and returns a <see cref="AuthContext"/>. 
//         /// </summary>
//         /// <param name="self"></param>
//         /// <param name="grantType">
//         ///   Specifies the requested <see cref="GrantType"/>.
//         /// </param>
//         /// <param name="options">
//         ///   Options describing the request.
//         /// </param>
//         /// <returns></returns>
//         public static async Task<Outcome<AuthContext>> GetAuthContextAsync(this ITetraPakConfiguration self, GrantType grantType, GrantOptions options)
//         {
//             if (string.IsNullOrWhiteSpace(options.Service))
//                 return Outcome<AuthContext>.Success(new AuthContext(grantType, self, options));
//
//             var path = new ConfigPath(options.Service);
//             if (path.Count < 2)
//             {
//                 path = path.Insert(ConfigurationSectionNames.Services);
//             }
//
//             return await self.GetSectionAsync(path) is not IServiceAuthConfig section 
//                 ? Outcome<AuthContext>.Fail(new ConfigurationException($"Could not find configured service \"{path}\"")) 
//                 : Outcome<AuthContext>.Success(new AuthContext(grantType, section, options));
//         }
//     }
//
// }