namespace TetraPak.XP.Configuration
{
    public interface IConfigurationValueDelegate
    {
        bool IsFallbackDelegate { get; }

        Outcome<T> GetValue<T>(ConfigurationValueArgs<T> args);
    }
}