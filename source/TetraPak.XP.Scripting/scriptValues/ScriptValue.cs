using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TetraPak.XP.StringValues;
#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace TetraPak.XP.Scripting;

public abstract class ScriptValue : IStringValue, IScriptValue
{
    static readonly List<IScriptValueFactory> s_factories = new() { new LiteralScriptValueFactory() };
        
    [ThreadStatic] static IScriptContext? s_scriptContext;

    internal static IScriptValueFactory[] Factories => s_factories.ToArray();
    
    protected IScriptContext Context => s_scriptContext ??= ScriptContext.Default;

    protected StringComparison StringComparison => Context.StringComparison;
    
    public abstract string ValueTypeIdentifier { get; }

    public abstract Task<bool> IsValidOperationAsync(ComparativeOperation operation);
    
    public string StringValue { get; }

    public static void AddParsers(params ScriptValueParser[] providers) => ScriptValueParser.AddProviders(providers);
    
    public static void AddFactory(params IScriptValueFactory[] factories) => s_factories.AddRange(factories);
    
    public static void WithContext(IScriptContext context) => s_scriptContext = context;
    
    public abstract Task<Outcome> Equals(ScriptValue value);

    public abstract Task<Outcome> NotEquals(ScriptValue value);

    public abstract Task<Outcome> LessThan(ScriptValue value);

    public abstract Task<Outcome> LessThanOrEquals(ScriptValue value);
    
    public abstract Task<Outcome> GreaterThan(ScriptValue value);

    public abstract Task<Outcome> GreaterThanOrEquals(ScriptValue value);

    public abstract bool TryCastTo<T>(
#if NET5_0_OR_GREATER 
        [NotNullWhen(true)] 
#endif            
        out T? value);

    protected ScriptValue(string stringValue)
    {
        StringValue = stringValue;
    }
}

public interface IScriptValue
{
    Task<Outcome> Equals(ScriptValue value);

    Task<Outcome> NotEquals(ScriptValue value);

    Task<Outcome> LessThan(ScriptValue value);

    Task<Outcome> LessThanOrEquals(ScriptValue value);
    
    Task<Outcome> GreaterThan(ScriptValue value);

    Task<Outcome> GreaterThanOrEquals(ScriptValue value);
}