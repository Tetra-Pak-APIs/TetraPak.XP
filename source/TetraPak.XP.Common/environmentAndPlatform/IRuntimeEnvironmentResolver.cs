using System;

namespace TetraPak.XP
{
    /// <summary>
    ///   Classes implementing this interface can be relied on to resolve the current runtime environment,
    /// </summary>
    public interface IRuntimeEnvironmentResolver
    {
        RuntimeEnvironment ResolveRuntimeEnvironment(RuntimeEnvironment useDefault = RuntimeEnvironment.Unknown);
    }

    public class RuntimeEnvironmentResolver : IRuntimeEnvironmentResolver
    {
        protected const string DefaultTetraPakAppEnvironmentVariable = "DOTNET_ENVIRONMENT";

        public virtual RuntimeEnvironment ResolveRuntimeEnvironment(RuntimeEnvironment useDefault = RuntimeEnvironment.Unknown)
        {
            var s = Environment.GetEnvironmentVariable(DefaultTetraPakAppEnvironmentVariable);
            if (!s.IsAssigned())
                return useDefault;

            return Enum.TryParse<RuntimeEnvironment>(s, true, out var value)
                ? value
                : useDefault;
        }
    }
        
}