using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TetraPak.XP.Scripting;

public sealed class ScriptContext : IScriptContext, IScriptValueProvider
{
    static IScriptContext? s_default;

    ScriptValueProvider Values { get; }

    public bool IsDefaultContext { get; private set; }
    
    public StringComparison StringComparison { get; set; }
    
    public Task<Outcome<object>> ResolveValueAsync(string key) => Values.GetValueAsync(key);

    public static IScriptContext Default
    {
        get => s_default ??= new ScriptContext { IsDefaultContext = true };
        set => s_default = value;
    }

    public Task<Outcome> ContainsValueAsync(string key) => Values.ContainsValueAsync(key);

    public Task<Outcome> RemoveValueAsync(string key) => Values.RemoveValueAsync(key);

    public Task<Outcome> SetValueAsync(string key, object? value, bool overwrite = false) => Values.SetValueAsync(key, value, overwrite);

    public Task<Outcome<object>> GetValueAsync(string key) => Values.GetValueAsync(key);

    public ScriptContext(ScriptValueProvider? values = null)
    {
        Values = values ?? new ScriptValueProvider();
        StringComparison = StringComparison.Ordinal;
    }
}

public class ScriptValueProvider : IScriptValueProvider
{
    readonly Dictionary<string, object?> _values = new();

    public virtual Task<Outcome> ContainsValueAsync(string key)
        =>
        Task.FromResult(_values.ContainsKey(key) 
            ? Outcome.Success()
            : Outcome.Fail(new KeyNotFoundException($"Key not found: '{key}'")));

    public virtual Task<Outcome> RemoveValueAsync(string key)
        =>
        Task.FromResult(_values.Remove(key)
            ? Outcome.Success()
            : Outcome.Fail(new Exception($"Could not remove value: '{key}'"))); 

    public virtual Task<Outcome> SetValueAsync(string key, object? value, bool overwrite = false)
    {
        if (_values.TryGetValue(key, out _) && !overwrite)
            return Task.FromResult(Outcome.Fail($"Cannot set value '{key}'"));

        _values[key] = value;
        return Task.FromResult(Outcome.Success());
    }

    public virtual Task<Outcome<object>> GetValueAsync(string key) =>
        Task.FromResult(_values.TryGetValue(key, out var value)
            ? Outcome<object>.Success(value!)
            : Outcome<object>.Fail(new KeyNotFoundException($"Value not found: {key}")));
}

public interface IScriptValueProvider
{
    Task<Outcome> ContainsValueAsync(string key);

    public Task<Outcome> RemoveValueAsync(string key);
    
    Task<Outcome> SetValueAsync(string key, object? value, bool overwrite = false);

    Task<Outcome<object>> GetValueAsync(string key);
}