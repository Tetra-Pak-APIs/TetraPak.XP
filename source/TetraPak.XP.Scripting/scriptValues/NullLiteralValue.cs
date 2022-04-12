using System;
using System.Threading.Tasks;

namespace TetraPak.XP.Scripting;

public sealed class NullLiteralValue : ScriptValue
{
    public const string Identifier = "null";
    
    public override string ValueTypeIdentifier => "null";
    
    public override Task<bool> IsValidOperationAsync(ComparativeOperation operation)
    {
        throw new System.NotImplementedException();
    }

    public override Task<Outcome> Equals(ScriptValue value)
    {
        throw new System.NotImplementedException();
    }

    public override Task<Outcome> NotEquals(ScriptValue value)
    {
        throw new System.NotImplementedException();
    }

    public override Task<Outcome> LessThan(ScriptValue value)
    {
        throw new System.NotImplementedException();
    }

    public override Task<Outcome> LessThanOrEquals(ScriptValue value)
    {
        throw new System.NotImplementedException();
    }

    public override Task<Outcome> GreaterThan(ScriptValue value)
    {
        throw new System.NotImplementedException();
    }

    public override Task<Outcome> GreaterThanOrEquals(ScriptValue value)
    {
        throw new System.NotImplementedException();
    }

    public override bool TryCastTo<T>(out T? value) where T : default
    {
        throw new System.NotImplementedException();
    }
    
    public NullLiteralValue(string stringValue) : base(stringValue)
    {
    }
}