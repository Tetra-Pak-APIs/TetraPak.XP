namespace TetraPak.XP.Configuration
{
    public delegate bool ValueParser<T>(string? stringValue, out T value);
}