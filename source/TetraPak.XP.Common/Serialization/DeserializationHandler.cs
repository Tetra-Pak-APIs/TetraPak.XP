namespace TetraPak.XP.Serialization
{
    public delegate bool DeserializationHandler(string serialized, out object deserialized, bool resolvedDynamicValue);
}