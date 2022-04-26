namespace TetraPak.XP.Configuration
{
    public delegate bool ValueParser(
        string? stringValue, 
        System.Type targetType, 
        out object? value,
        object defaultValue);
}