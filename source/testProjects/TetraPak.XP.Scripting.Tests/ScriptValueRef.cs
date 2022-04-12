using System;
using System.Threading.Tasks;
using TetraPak.XP.StringValues;

namespace TetraPak.XP.Scripting.Tests;

sealed class ScriptValueRef : StringValueBase
{
    public string Identifier { get; }

    public ScriptValue? IndexerValue { get; }

    public ScriptValueRef? Member { get; private set; }

    internal Task<Outcome<object>> ResolveValueAsync(IScriptContext context)
    {
        return context.ResolveValueAsync(Identifier);
    }

    async Task<Outcome<object>> resolveValueAsync(IScriptValueProvider values)
    {
        var outcome = await values.GetValueAsync(Identifier);
        if (!outcome)
            return outcome;
        
        // if (IndexerValue is { })
        // {
        //     var indexerOutcome = await resolveValueAsync(, IndexerValue);
        // }

        throw new NotImplementedException();

        // aaa = Value (from context)
        // aaa.bbb = 
        // aaa[xxx.yyy]
        // aaa[xxx.yyy].bbb
    }

    async Task<Outcome<object>> resolveValueAsync(IScriptValueProvider scriptValues, ScriptValue value)
    {
        throw new NotImplementedException();
    }

    public void SetMember(ScriptValueRef scriptValueRef) => Member = scriptValueRef;

    // public override async Task<bool> IsValidOperationAsync(ComparativeOperation operation)
    // {
    //     return operation switch
    //     {
    //         ComparativeOperation.None => false,
    //         ComparativeOperation.Equal => true,
    //         ComparativeOperation.NotEqual => true,
    //         ComparativeOperation.LessThan => await IsLessOrGreaterComparisonSupportedAsync(),
    //         ComparativeOperation.LessThanOrEquals => await IsLessOrGreaterComparisonSupportedAsync(),
    //         ComparativeOperation.GreaterThan => await IsLessOrGreaterComparisonSupportedAsync(),
    //         ComparativeOperation.GreaterThanOrEquals => await IsLessOrGreaterComparisonSupportedAsync(),
    //         ComparativeOperation.Contains => await IsCollectionAsync(),
    //         ComparativeOperation.ContainsOrEqual => await IsCollectionAsync(),
    //         ComparativeOperation.NotContains => await IsCollectionAsync(),
    //         ComparativeOperation.Contained => true,
    //         ComparativeOperation.NotContained => true,
    //         ComparativeOperation.ContainedOrEqual => true,
    //         _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
    //     };
    // }

    // public override async Task<Outcome> Equals(ScriptValue value)
    // {
    //     var outcome = await resolveValueAsync();
    //     if (!outcome)
    //         return Outcome.Fail($"{this} could not be resolved");
    //     
    //     throw new NotImplementedException();
    // }

    //
    // async Task<bool> IsLessOrGreaterComparisonSupportedAsync()
    // {
    //     if (Context.IsDefaultContext)
    //         return true;
    //     
    //     var outcome = await ResolveValueAsync(Context);
    //     if (!outcome)
    //         return true;
    //
    //     var value = outcome.Value!;
    //     return value.GetType().IsLessOrGreaterComparisonSupported();
    // }

    
    internal ScriptValueRef(string stringValue, string identifier, ScriptValue? indexerValue) 
    : base(stringValue)
    {
        Identifier = identifier;
        IndexerValue = indexerValue;
    }

}