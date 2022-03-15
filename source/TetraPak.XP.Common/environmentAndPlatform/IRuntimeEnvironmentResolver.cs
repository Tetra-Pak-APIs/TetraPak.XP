namespace TetraPak.XP
{
    /// <summary>
    ///   Classes implementing this interface can be relied on to resolve the current runtime environment,
    /// </summary>
    public interface IRuntimeEnvironmentResolver
    {
        RuntimeEnvironment ResolveRuntimeEnvironment(RuntimeEnvironment useDefault = RuntimeEnvironment.Unknown);
    }
}