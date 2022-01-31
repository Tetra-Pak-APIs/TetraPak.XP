using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.Logging;

[assembly:InternalsVisibleTo("TetraPak.XP.DependencyInjection.Tests")]

namespace TetraPak.XP.DependencyInjection
{
    public static class XpServices
    {
        static readonly object s_syncRoot = new();
        static readonly Dictionary<Type, ResolutionInfo> s_resolved = new();
        static readonly Dictionary<Type, ResolutionInfo> s_registered = new();
        static IServiceCollection? s_serviceCollection;
        static IServiceProvider? s_provider;

        public static bool IsDefaultSingleton { get; set; } = true;

        class ResolutionInfo
        {
            /// <summary>
            ///   Specifies whether the service is a singleton.
            /// </summary>
            public bool IsSingleton { get; set; }

            /// <summary>
            ///   The service type.
            /// </summary>
            public Type Type { get; set; }
            
            /// <summary>
            ///   A resolved instance. 
            /// </summary>
            public object? Service { get; set; }

            /// <summary>
            ///   When set, clients will only get this service when requesting it explicitly
            ///   (not by base class or interface).
            /// </summary>
            public bool IsTypeLiteral { get; }

            public ResolutionInfo(Type type, bool isSingleton, bool isTypeLiteral)
            {
                Type = type;
                IsSingleton = isSingleton;
                Service = null!;
                IsTypeLiteral = isTypeLiteral;
            }

        }

        /// <summary>
        ///   Registers a service implementation for a specified type, explicitly or implicitly.
        /// </summary>
        /// <param name="type">
        ///   The service type to be registered.  
        /// </param>
        /// <param name="throwOnConflict">
        ///   (optional; default=<c>true</c>)<br/>
        ///   Specifies whether to throw an exception if the type was already registered;
        ///   Otherwise the request is simply ignored.
        /// </param>
        /// <param name="isTypeLiteral">
        ///   (optional; default=<c>false</c>)<br/>
        ///   When set, this flag requires a client to ask for the registered type literally. Requesting a
        ///   base class or implemented interface should not resolve to this type.
        /// </param>
        /// <exception cref="ArgumentException">
        ///   The type was an interface or abstract class (must be a concrete class or value type).
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///   The <paramref name="type"/> was already registered -and- the <paramref name="throwOnConflict"/> was set. 
        /// </exception>
        public static void Register(Type type, bool throwOnConflict = true, bool isTypeLiteral = false)
        {
            if (type.IsAbstract)
                throw new ArgumentException(
                    $"Cannot register abstract types with {typeof(XpServices)}. Only concrete implementations are allowed");

            if (s_registered.ContainsKey(type))
            {
                if (!throwOnConflict)
                    return;
                
                throw new InvalidOperationException($"Type was already registered: {type}");
            }
            
            s_registered.Add(type, new ResolutionInfo(type, IsDefaultSingleton, isTypeLiteral));
        }

        /// <summary>
        ///   Registers a service implementation for a specified type, explicitly or implicitly.
        /// </summary>
        /// <exception cref="ArgumentException">
        ///   The type was an interface or abstract class (must be a concrete class or value type).
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///   The type was already registered. 
        /// </exception>
        public static void Register<T>() => Register(typeof(T));
        
        /// <summary>
        ///   Registers a literal service type (see remarks).
        /// </summary>
        /// <typeparam name="T">
        ///   The service type to be registered. 
        /// </typeparam>
        /// <remarks>
        ///   A literal service is only resolved when requested "literally". Requesting a base class
        ///   or an interface implemented by the service will return an unsuccessful outcome. 
        /// </remarks>
        public static void RegisterLiteral<T>() => Register(typeof(T), isTypeLiteral:true);
        
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
            var outcome = TryGet(typeof(T), true);
            if (!outcome)
                return Outcome<T>.Fail(outcome.Message, outcome.Exception!);
            
            if (outcome.Value is T tValue)
                return Outcome<T>.Success(tValue);
                
            return Outcome<T>.Fail(
                new InvalidOperationException ($"Resolved service ({outcome.Value}) is not of expected type: {typeof(T)}"));
        }
        
        internal static Outcome<object> TryGet(Type type, bool tryServiceProvider)
        {
            // note the 'cannotResolve' list of types is needed to avoid infinite recursion 
            return tryGet(type, tryServiceProvider, new List<Type>());
        }

        static Outcome<object> tryGet(Type type, bool tryServiceProvider, List<Type> cannotResolve)
        {
            if (cannotResolve.Contains(type))
                return Outcome<object>.Fail(failCannotResolveServiceFor(type));

            // first try get a resolved, literal, explicit type ...
            if (s_resolved.TryGetValue(type, out var info))
                return info.IsSingleton 
                    ? Outcome<object>.Success(info.Service!)
                    : tryActivate(info.Type, cannotResolve);
            
            // fall back to dynamically resolving a service from the requested type ...
            return tryResolve(type, tryServiceProvider, cannotResolve);
        }

        static Outcome<object> tryResolve(Type type, bool tryServiceProvider, List<Type> cannotResolve)
        {
            Outcome<object> activateOutcome = null!;

            // start by looking for the registered, literal, type ...
            if (!type.IsInterface && !type.IsAbstract && s_registered.TryGetValue(type, out var info))
            {
                if (!info.IsTypeLiteral || type == info.Type)
                    return activate(info.Type, info);
            }
            
            // next, try IServiceProvider if available ...
            if (tryServiceProvider && s_provider is { })
            {
                var service = 
                s_provider is XpServiceProvider xpServiceProvider
                    ? xpServiceProvider.GetService(type, false)
                    : s_provider.GetService(type);
                if (service is { })
                    return Outcome<object>.Success(service);
            }
            
            // finally, fall back to dynamically resolving from XpServices implementation ...
            foreach (var pair in s_registered)
            {
                var registeredType = pair.Key;
                var registrationInfo = pair.Value;
                
                if (!type.Is(registeredType))
                    continue;

                if (registrationInfo.IsTypeLiteral && type != registeredType)
                    continue;

                return activate(registeredType, registrationInfo);
                // activateOutcome = tryActivate(registeredType, cannotResolve); obsolete
                // if (!activateOutcome)
                //     continue;
                //
                // var service = activateOutcome.Value!;
                // var resolution = new ResolutionInfo(type, registrationInfo.IsSingleton, false);
                // if (registrationInfo.IsSingleton)
                // {
                //     resolution.Service = service;
                // }
                // s_resolved.Add(type, resolution);
                // return Outcome<object>.Success(service);
            }
            
            return activateOutcome ?? Outcome<object>.Fail(failCannotResolveServiceFor(type));

            Outcome<object> activate(Type svcType, ResolutionInfo svcInfo)
            {
                activateOutcome = tryActivate(svcType, cannotResolve);
                if (!activateOutcome)
                    return Outcome<object>.Fail(failCannotResolveServiceFor(svcType));
            
                var resolution = new ResolutionInfo(type, svcInfo.IsSingleton, false);
                if (svcInfo.IsSingleton)
                {
                    resolution.Service = activateOutcome.Value!;
                }
                s_resolved.Add(type, resolution);
                return Outcome<object>.Success(activateOutcome.Value!);
            }
        }

        static Exception failCannotResolveServiceFor(Type type) => new($"Cannot resolve service that implements {type}");

        static Outcome<object> tryActivate(Type type, List<Type> cannotResolve) 
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
                    cannotResolve.Add(type);
                    return Outcome<object>.Fail($"Failed when activating service {type}", ex);
                }

            foreach (var constructor in type.GetConstructors())
            {
                var parameterInfos = constructor.GetParameters();
                var parameters = new List<object>();
                var isMatchingArgs = true;
                foreach (var parameter in parameterInfos)
                {
                    var outcome = tryGet(parameter.ParameterType, false, cannotResolve);
                    if (!outcome)
                    {
                        isMatchingArgs = false;
                        break;
                    }
                    
                    parameters.Add(outcome.Value!);
                }
                
                if (!isMatchingArgs)
                    continue;

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
            
            cannotResolve.Add(type);
            return Outcome<object>.Fail($"Failed when activating service {type}", new Exception("Could not resolve a suitable constructor"));
        }

        // public static void Init(IServiceCollection serviceCollection)
        // {
        //     // todo if needed
        // }

        public static IServiceCollection RegisterXpServices(this IServiceCollection services, ILog? log = null)
        {
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (var i = 0; i < assemblies.Length; i++)
                {
                    // fetch all [XpDependency] attributes (invokes their ctor which registers their specified Type) ...
                    var asm = assemblies[i];
                    asm.GetCustomAttributes<XpServiceAttribute>();
                }

                return services;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw;
            }
        }

        public static IServiceProvider BuildXpServiceProvider(this IServiceCollection serviceCollection)
        {
            return BuildXpServiceProvider(serviceCollection, new ServiceProviderOptions());
        }

        /// <summary>
        /// Creates a <see cref="ServiceProvider"/> containing services from the provided <see cref="IServiceCollection"/>
        /// optionally enabling scope validation.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/> containing service descriptors.</param>
        /// <param name="validateScopes">
        /// <c>true</c> to perform check verifying that scoped services never gets resolved from root provider; otherwise <c>false</c>.
        /// </param>
        /// <returns>The <see cref="ServiceProvider"/>.</returns>
        public static IServiceProvider BuildXpServiceProvider(this IServiceCollection serviceCollection, bool validateScopes)
        {
            return serviceCollection.BuildXpServiceProvider(new ServiceProviderOptions { ValidateScopes = validateScopes });
        }
        
        public static IServiceProvider BuildXpServiceProvider(this IServiceCollection serviceCollection, ServiceProviderOptions options)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            lock (s_syncRoot)
            {
                return s_provider ??= new XpServiceProvider(serviceCollection, options);
            }
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

        internal static void Reset()
        {
            lock (s_syncRoot)
            {
                s_provider = null;
                s_serviceCollection = null;
                s_registered.Clear();
                s_resolved.Clear();
            }
        }
    }
}