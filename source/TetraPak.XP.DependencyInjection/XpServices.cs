using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace TetraPak.XP.SimpleDI
{
    public static class XpServices
    {
        static readonly object s_syncRoot = new();
        static readonly Dictionary<Type, ResolutionInfo> s_resolved = new();
        static readonly Dictionary<Type, ResolutionInfo> s_registered = new();
        static IServiceCollection? s_serviceCollection;

        public static bool IsDefaultSingleton { get; set; } = true;

        class ResolutionInfo
        {
            public bool IsSingleton { get; set; }

            public Type Type { get; set; }
            
            public object Service { get; set; }
        }

        public static void Register(Type type, bool throwOnConflict = true)
        {
            if (type.IsInterface)
                throw new InvalidOperationException(
                    $"Cannot register interfaces with {typeof(XpServices)} (only implementations are allowed)");

            if (s_registered.ContainsKey(type))
            {
                if (!throwOnConflict)
                    return;
                
                throw new InvalidOperationException($"Type was already registered: {type}");
            }
            
            s_registered.Add(type, new ResolutionInfo { IsSingleton = IsDefaultSingleton });
        }

        public static void Register<T>() => Register(typeof(T));
        
        public static T? Get<T>()
        {
            var outcome = TryGet<T>();
            return outcome ? outcome.Value : default;
        }

        public static T GetRequired<T>()
        {
            var outcome = TryGet<T>();
            return outcome 
                ? outcome.Value! 
                : throw new Exception($"Cannot resolve service: {typeof(T)}");
        }

        public static Outcome<T> TryGet<T>()
        {
            var outcome = TryGet(typeof(T));
            if (!outcome)
                return Outcome<T>.Fail(outcome.Message, outcome.Exception!);
            
            if (outcome.Value is T tValue)
                return Outcome<T>.Success(tValue);
                
            return Outcome<T>.Fail(
                new InvalidOperationException ($"Resolved service ({outcome.Value}) is not of expected type: {typeof(T)}"));
        }
        
        internal static Outcome<object> TryGet(Type type)
        {
            if (s_resolved.TryGetValue(type, out var info))
                return info.IsSingleton 
                    ? Outcome<object>.Success(info.Service)
                    : tryActivate(info.Type);

            return tryResolve(type);
        }

        static Outcome<object> tryResolve(Type type)
        {
            Outcome<object> activateOutcome = null!;
            foreach (var pair in s_registered)
            {
                var registeredType = pair.Key;
                var registrationInfo = pair.Value;
                if (!type.IsAssignableFrom(registeredType))
                    continue;
                
                activateOutcome = tryActivate(registeredType);
                if (!activateOutcome)
                    continue;
                    
                var resolution = new ResolutionInfo { Type = type };
                if (registrationInfo.IsSingleton)
                {
                    resolution.Service = activateOutcome.Value!;
                    resolution.IsSingleton = true;
                }
                s_resolved.Add(type, resolution);
                return Outcome<object>.Success(resolution.Service);
            }
            
            return activateOutcome 
                   ?? Outcome<object>.Fail(
                       new InvalidOperationException($"Cannot resolve service that implements {type}"));
        }

        // static Outcome<object> tryResolveFromInterface(Type iface)
        // {
        //     foreach (var pair in s_registered)
        //     {
        //         var registeredType = pair.Key;
        //         var registrationInfo = pair.Value;
        //         if (!registeredType.IsImplementingInterface(iface))
        //             continue;
        //         
        //         var outcome = tryActivate(registeredType);
        //         var resolution = new ResolutionInfo
        //         {
        //             Type = iface, 
        //         };
        //         if (registrationInfo.IsSingleton)
        //         {
        //             resolution.Service = outcome.Value;
        //             resolution.IsSingleton = true;
        //         }
        //         s_resolved.Add(iface, resolution);
        //     }
        //     
        //     return Outcome<object>.Fail($"Cannot resolve service that implements {iface}");
        // }

        static Outcome<object> tryActivate(Type type)
        {
            var emptyCtor = type.GetConstructor(Type.EmptyTypes);
            if (emptyCtor is { })
                try
                {
                    var service = emptyCtor.Invoke(Array.Empty<object>());
                    return Outcome<object>.Success(service);
                }
                catch (Exception ex)
                {
                    return Outcome<object>.Fail($"Failed when activating service {type}", ex);
                }

            foreach (var constructor in type.GetConstructors())
            {
                var parameterInfos = constructor.GetParameters();
                var parameters = new List<object>();
                foreach (var parameter in parameterInfos)
                {
                    var outcome = TryGet(parameter.ParameterType);
                    if (!outcome)
                        break;
                    
                    parameters.Add(outcome.Value!);
                }

                try
                {
                    var service = constructor.Invoke(parameters.ToArray());
                    return Outcome<object>.Success(service);
                }
                catch
                {
                    // ignored
                }
            }
            
            return Outcome<object>.Fail($"Failed when activating service {type}", new Exception("Could not resolve a suitable constructor"));
        }

        // public static void Init(IServiceCollection serviceCollection)
        // {
        //     // todo if needed
        // }

        public static IServiceCollection RegisterXpDependencies(this IServiceCollection services)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (var i = 0; i < assemblies.Length; i++)
            {
                // fetch all [XpDependency] attributes (invokes their ctor which registers their specified Type) ...
                var asm = assemblies[i];
                asm.GetCustomAttributes<XpDependencyAttribute>();
            }

            return services;
        }

        public static IServiceProvider BuildXpServiceProvider(this IServiceCollection services)
        {
            return BuildXpServiceProvider(services, new ServiceProviderOptions());
        }

        /// <summary>
        /// Creates a <see cref="ServiceProvider"/> containing services from the provided <see cref="IServiceCollection"/>
        /// optionally enabling scope validation.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> containing service descriptors.</param>
        /// <param name="validateScopes">
        /// <c>true</c> to perform check verifying that scoped services never gets resolved from root provider; otherwise <c>false</c>.
        /// </param>
        /// <returns>The <see cref="ServiceProvider"/>.</returns>
        public static IServiceProvider BuildXpServiceProvider(this IServiceCollection services, bool validateScopes)
        {
            return services.BuildXpServiceProvider(new ServiceProviderOptions { ValidateScopes = validateScopes });
        }
        
        public static IServiceProvider BuildXpServiceProvider(this IServiceCollection services, ServiceProviderOptions options)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return new XpServiceProvider(services, options);
        }

        public static IServiceCollection GetServiceCollection()
        {
            lock (s_syncRoot)
            {
                if (s_serviceCollection is { })
                    return s_serviceCollection;

                return s_serviceCollection = new ServiceCollection();
            }
        }
        
        public static IServiceCollection NewServiceCollection(bool useExisting)
        {
            lock (s_syncRoot)
            {
                if (!useExisting)
                    return new XpServiceCollection();
                    
                if (s_serviceCollection is { })
                    return s_serviceCollection;

                return s_serviceCollection = new XpServiceCollection();
            }
        }
    }
}