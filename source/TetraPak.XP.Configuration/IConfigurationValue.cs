namespace TetraPak.XP.Configuration
{
    public interface IConfigurationValue : IConfigurationItem
    {
        /// <summary>
        /// Gets or sets the section value.
        /// </summary>
        object? Value { get; set; }
    }
}