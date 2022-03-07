// using System.IO;
// using Microsoft.Extensions.DependencyInjection;
// using TetraPak.XP.Auth.Abstractions;
// using TetraPak.XP.Configuration;
//
// namespace TetraPak.XP.Auth
// {
//     public static class TetraPakConfigServiceHelper obsolete
//     {
//         static bool s_isConfigurationAdded;
//         static readonly object s_syncRoot = new();
//
//         /// <summary>
//         ///   (fluent api)<br/>
//         ///   Adds a <see cref="TetraPakConfig"/> (singleton) service to the <see cref="IServiceCollection"/>
//         ///   and then returns the service collection.
//         /// </summary>
//         /// <param name="collection">
//         ///   The service collection.
//         /// </param>
//         /// <param name="folder">
//         ///   (optional; default=current folder)<br/>
//         ///   A folder to read configuration from.
//         /// </param>
//         /// <returns>
//         ///   The service <paramref name="collection"/>.
//         /// </returns>
//         public static IServiceCollection AddTetraPakConfiguration(
//             this IServiceCollection collection, 
//             DirectoryInfo? folder = null)
//         {
//             lock (s_syncRoot)
//             {
//                 if (s_isConfigurationAdded)
//                     return collection;
//
//                 s_isConfigurationAdded = true;
//             }
//
//             // collection.AddConfiguration(folder);
//             collection.AddSingleton<ITetraPakConfiguration,TetraPakConfig>();
//             return collection;
//         }
//     }
// }