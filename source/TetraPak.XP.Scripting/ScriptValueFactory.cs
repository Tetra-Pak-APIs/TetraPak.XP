namespace TetraPak.XP.Scripting;

public interface IScriptValueFactory
{
    Outcome<ScriptValue> GetScriptValue(string key, object? value);
}