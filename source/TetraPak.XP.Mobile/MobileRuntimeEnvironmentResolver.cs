using System;
using System.Reflection;
using TetraPak.XP;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Mobile;

[assembly:XpService(typeof(IRuntimeEnvironmentResolver), typeof(MobileRuntimeEnvironmentResolver))]

namespace TetraPak.XP.Mobile
{
    class MobileRuntimeEnvironmentResolver : RuntimeEnvironmentResolver
    {
        public override RuntimeEnvironment ResolveRuntimeEnvironment(RuntimeEnvironment useDefault = RuntimeEnvironment.Unknown)
        {
            resolvedFromAssembly();
            return base.ResolveRuntimeEnvironment(useDefault);
        }

        static void resolvedFromAssembly()
        {
            var attribute = Assembly.GetEntryAssembly()?.GetCustomAttribute<TetraPakRuntimeEnvironmentAttribute>();
            if (attribute is null)
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    attribute = assembly.GetCustomAttribute<TetraPakRuntimeEnvironmentAttribute>();
                    if (attribute is {})
                        break;
                }
            }
            if (attribute is {})
            {
                Environment.SetEnvironmentVariable(
                DefaultTetraPakAppEnvironmentVariable, 
                 attribute.RuntimeEnvironment.ToString());
            }
        }
    }
}