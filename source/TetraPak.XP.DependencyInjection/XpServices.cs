using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        static readonly HashSet<Assembly> s_registeredAssemblies = new();
        // static readonly HashSet<Type> s_registeredImplicitInterfaces = new();// Experiment concept 

        public static bool DefaultIsSingleton { get; set; } = true;

        // public static bool IsDefaultImplementingSingleInterface { get; set; } = true; // Experiment concept 

        class ResolutionInfo
        {
            /// <summary>
            ///   Specifies whether the service is a singleton.
            /// </summary>
            public bool IsSingleton { get; }

            /// <summary>
            ///   The service type.
            /// </summary>
            public Type Type { get; }
            
            /// <summary>
            ///   The service type.
            /// </summary>
            public Type? ImplementingType { get; }
            
            /// <summary>
            ///   A resolved instance. 
            /// </summary>
            public object? Service { get; set; }

            /// <summary>
            ///   When set, clients will only get this service when requesting it explicitly
            ///   (not by base class or interface).
            /// </summary>
            public bool IsTypeLiteral { get; }

            internal ResolutionInfo(Type type, bool isSingleton, bool isTypeLiteral)
            {
                Type = type;
                IsSingleton = isSingleton;
                Service = null!;
                IsTypeLiteral = isTypeLiteral;
            }
            
            internal ResolutionInfo(Type type, Type implementingType, bool isSingleton)
            {
                Type = type;
                ImplementingType = implementingType;
                IsSingleton = isSingleton;
                Service = null!;
                IsTypeLiteral = false;
            }
        }

        /// <summary>
        ///   Registers a service implementation for a specified type, explicitly or implicitly.
        /// </summary>
        /// <param name="implementingType">
        ///   The service type to be registered.  
        /// </param>
        /// <param name="skipOnConflict">
        ///   (optional; default=<c>true</c>)<br/>
        ///   Specifies whether to ignore the request if the type was already registered;
        ///   Otherwise an exception will be thrown.
        /// </param>
        /// <param name="isTypeLiteral">
        ///   (optional; default=<c>false</c>)<br/>
        ///   When set, this flag requires a client to ask for the registered type literally. Requesting a
        ///   base class or implemented interface should not resolve to this type.
        /// </param>
        /// <param name="isSingleton">
        ///   (optional; default=<see cref="DefaultIsSingleton"/>)<br/>
        ///   Specifies whether the service is a singleton.
        /// </param>
        /// <exception cref="ArgumentException">
        ///   The type was an interface or abstract class (must be a concrete class or value type).
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///   The <paramref name="implementingType"/> was already registered -and- the <paramref name="skipOnConflict"/> was not set. 
        /// </exception>
        public static void Register(
            Type implementingType, 
            bool skipOnConflict = true, 
            bool isTypeLiteral = false,
            bool? isSingleton = null) 
            =>
            Register(implementingType, null!, skipOnConflict, isTypeLiteral, isSingleton);

        public static void Register(
            Type implementingType, 
            Type? interfaceType, 
            bool skipOnConflict = true, 
            bool isTypeLiteral = false, 
            bool? isSingleton = null)
        {
            // if (IsDefaultImplementingSingleInterface && !isTypeLiteral) // Experimental concept
            // {
            //     // check for single interface and assume this type is the implementation of it ...
            //     var interfaces = implementingType.GetInterfaces();
            //     if (interfaces.Length == 1 && !s_registeredImplicitInterfaces.Contains(interfaces[0]))
            //     {
            //         interfaceType = interfaces[0];
            //         s_registeredImplicitInterfaces.Add(interfaceType);
            //     }
            // }
            
            var isIndeedSingleton = isSingleton ?? DefaultIsSingleton; // :-)
            if (implementingType.IsAbstract)
                throw new ArgumentException(
                    $"Cannot register abstract types with {typeof(XpServices)}. Only concrete implementations are allowed");
            
            if (interfaceType is {} && isTypeLiteral)
                throw new InvalidOperationException(
                    $"Cannot register {nameof(interfaceType)} and also set {nameof(isTypeLiteral)}");

            if (s_registered.ContainsKey(implementingType))
            {
                if (skipOnConflict)
                    return;

                throw new InvalidOperationException($"Type was already registered: {implementingType}");
            }

            if (s_serviceCollection is { } && interfaceType is { })
            {
                if (isIndeedSingleton)
                {
                    s_serviceCollection.TryAddSingleton(interfaceType, implementingType);
                }
                else
                {
                    s_serviceCollection.TryAddTransient(interfaceType, implementingType);
                }
            }

            s_registered.Add(implementingType,
                interfaceType is { }
                    ? new ResolutionInfo(implementingType, interfaceType, isTypeLiteral)
                    : new ResolutionInfo(implementingType, isIndeedSingleton, isTypeLiteral));

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
        public static void RegisterLiteral<T>(bool skipOnConflict = true) => Register(typeof(T), skipOnConflict, true);
        
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
                return Outcome<T>.Fail(outcome.Exception!);
            
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
                
                if (!type.Is(registeredType, false))
                    continue;

                if (registrationInfo.IsTypeLiteral && type != registeredType)
                    continue;

                return activate(registeredType, registrationInfo);
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
                    return Outcome<object>.Fail( new Exception($"Failed when activating service {type}", ex));
                }

            foreach (var constructor in type.GetConstructors())
            {
                var parameterInfos = constructor.GetParameters();
                var parameters = new List<object>();
                var isMatchingArgs = true;
                foreach (var parameter in parameterInfos)
                {
                    var outcome = tryGet(parameter.ParameterType, false, cannotResolve);
                    if (!outcome && !parameter.IsOptional)
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
            return Outcome<object>.Fail($"Failed when activating service {type}. Could not resolve a suitable constructor");
        }

        public static IServiceCollection RegisterXpServices(ILog? log = null)
            => RegisterXpServices(null, log);

        public static IServiceCollection RegisterXpServices(this IServiceCollection? collection, ILog? log = null)
        {
            try
            {
                collection ??= GetServiceCollection();
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (var i = 0; i < assemblies.Length; i++)
                {
                    // fetch all [assembly:XpService(...)] attributes (invokes their ctor which registers their specified Type) ...
                    var asm = assemblies[i];
                    if (s_registeredAssemblies.Contains(asm))
                        continue;
                    var  nisse = // nisse
                    asm.GetCustomAttributes<XpServiceAttribute>();
                    s_registeredAssemblies.Add(asm);
                }

                return collection;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw;
            }
        }

        public static IServiceProvider BuildXpServices() => BuildXpServices(null!);

        public static IServiceProvider BuildXpServices(this IServiceCollection? self)
        {
            var collection = RegisterXpServices(self);
            return BuildXpServiceProvider(collection);
        }

        /// <summary>
        ///   Forces loading assemblies for automatic service registration (see remarks).   
        /// </summary>
        /// <param name="types">
        ///   One or more types from assemblies that needs to be loaded before declarative
        ///   service registration (see remarks). 
        /// </param>
        /// <returns>
        ///   A <see cref="XpServicesBuilder"/> object to support the fluent code api. 
        /// </returns>
        /// <remarks>
        ///   As the <see cref="XpServices"/> api supports both "classic" procedure service
        ///   configuration as well as the declarative approach (using assembly-level
        ///   <see cref="XpServiceAttribute"/>s) you need a mechanism to ensure the required assemblies
        ///   are loaded prior to calling any of the configuration methods
        ///   (such as <see cref="BuildXpServices()"/> or (<see cref="RegisterXpServices(TetraPak.XP.Logging.ILog?)"/>
        /// </remarks>
        public static XpServicesBuilder Include(params Type[] types) =>new(types);

        public static XpPlatformServicesBuilder BuildFor(params Type[] types) => new(types);

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

        public static IServiceCollection GetServiceCollection(bool useExisting = true)
        {
            lock (s_syncRoot)
            {
                if (s_serviceCollection is null)
                {
                    s_serviceCollection = new ServiceCollection();
                    s_serviceCollection.RegisterXpServices();
                    return s_serviceCollection;
                }

                var collection = useExisting 
                    ? s_serviceCollection 
                    : new ServiceCollection();
                collection.RegisterXpServices();
                return collection;
            }
        }

        public static IServiceCollection UseServiceCollection(IServiceCollection collection, bool replace = false)
        {
            lock (s_syncRoot)
            {
                if (s_serviceCollection is { } && !replace)
                    throw new InvalidOperationException("A service collection is already in use");
                
                s_serviceCollection = collection;
                s_serviceCollection.RegisterXpServices();
                collection.RegisterXpServices();
                return s_serviceCollection;
            }
        }
        
        public static IServiceCollection NewServiceCollection()
        {
            lock (s_syncRoot)
            {
                return s_serviceCollection = new ServiceCollection();
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
    
    /// <summary>
    ///   This class is only used to provided a convenient fluid code api for supplying
    ///   needed services by pre-loading the declaring assemblies before the declarative
    ///   services declaration process is invoked.  
    /// </summary>
    public class XpServicesBuilder
    {
        // note This list is just to force the linker to load the required assemblies
        // ReSharper disable once NotAccessedField.Local
        Type[] _triggerAssemblyLoading;

        /// <summary>
        ///   Simply invokes the <see cref="XpServices.RegisterXpServices(TetraPak.XP.Logging.ILog?)"/> method.
        /// </summary>
        public IServiceCollection RegisterXpServices(IServiceCollection? collection) 
            => collection.RegisterXpServices();
                
        /// <summary>
        ///   Simply invokes the <see cref="XpServices.BuildXpServices()"/> method.
        /// </summary>
        public IServiceProvider BuildXpServices(IServiceCollection? collection = null) 
            => collection.BuildXpServices();
            
        public IServiceCollection GetServiceCollection() => XpServices.GetServiceCollection();

        public IServiceCollection WithServiceCollection(IServiceCollection collection) 
            =>
            XpServices.UseServiceCollection(collection);

        public XpServicesBuilder Include(params Type[] types)
        {
            _triggerAssemblyLoading = types;
            return this;
        }

        internal XpServicesBuilder(params Type[] types)
        {
            _triggerAssemblyLoading = types;
        }

    }

    public class XpPlatformServicesBuilder 
    {
        readonly Type[] _types;

        public XpServicesBuilder Build() => new(_types); 
        
        public XpPlatformServicesBuilder(Type[] types)
        {
            _types = types;
        }
    }
}