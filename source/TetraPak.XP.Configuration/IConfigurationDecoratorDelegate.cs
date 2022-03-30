namespace TetraPak.XP.Configuration;

public interface IConfigurationDecoratorDelegate
{
    bool IsFallbackDecorator { get; }

    Outcome<ConfigurationSectionDecorator> WrapSection(ConfigurationSectionDecoratorArgs args);
}