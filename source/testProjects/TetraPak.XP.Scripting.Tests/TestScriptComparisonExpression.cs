using System;
using System.Threading.Tasks;
using Xunit;

namespace TetraPak.XP.Scripting.Tests;

public sealed class TestComparisonExpression
{
    [Fact]
    public async Task Numeric_values_comparison()
    {
        Assert.True(await ScriptExpression.RunAsync("17 == 17.0000"));
        Assert.True(await ScriptExpression.RunAsync("17.0001 == 17.0001"));
        Assert.False(await ScriptExpression.RunAsync("12 == 42"));
        
        Assert.True(await ScriptExpression.RunAsync("17 != 42"));
        Assert.False(await ScriptExpression.RunAsync("17 != 17"));
        
        Assert.True(await ScriptExpression.RunAsync("17 < 42.0000"));
        Assert.True(await ScriptExpression.RunAsync("17 < 17.0001"));
        Assert.False(await ScriptExpression.RunAsync("42.0000 < 17"));
        Assert.True(await ScriptExpression.RunAsync("17 <= 42"));
        Assert.False(await ScriptExpression.RunAsync("42 <= 17"));
        
        Assert.True(await ScriptExpression.RunAsync("42.000 > 17.0000"));
        Assert.False(await ScriptExpression.RunAsync("17 > 42.0000"));
        Assert.True(await ScriptExpression.RunAsync("42.0001 > 42.0000"));
        Assert.True(await ScriptExpression.RunAsync("17 >= 17"));
        Assert.False(await ScriptExpression.RunAsync("17 >= 42"));
    }
    
    [Fact]
    public async Task String_values_comparison()
    {
        // equals ...
        var ignoreCase = new ScriptContext { StringComparison = StringComparison.OrdinalIgnoreCase };
        Assert.True(await ScriptExpression.RunAsync("\"abc\" == \"abc\""));
        Assert.False(await ScriptExpression.RunAsync("\"abc\" == \"Abc\""));
        Assert.True(await ScriptExpression.RunAsync("\"abc\" == \"Abc\"", ignoreCase));
        
        // not equals ...
        Assert.True(await ScriptExpression.RunAsync("\"abc\" != \"Abc\""));
        Assert.False(await ScriptExpression.RunAsync("\"abc\" != \"abc\""));
        Assert.False(await ScriptExpression.RunAsync("\"abc\" != \"Abc\"", ignoreCase));
        
        // less/greater than ...
        Assert.True(await ScriptExpression.RunAsync("\"abc\" < \"bcd\""));
        Assert.True(await ScriptExpression.RunAsync("\"abc\" <= \"bcd\""));
        Assert.True(await ScriptExpression.RunAsync("\"abc\" <= \"abc\""));
        Assert.False(await ScriptExpression.RunAsync("\"abc\" > \"bcd\""));
        Assert.False(await ScriptExpression.RunAsync("\"abc\" >= \"bcd\""));
        Assert.True(await ScriptExpression.RunAsync("\"abc\" >= \"abc\""));

        // contains ...
        var containsOutcome = await ScriptExpression.RunAsync<int>("\"abc\" { \"bc\"");
        Assert.True(containsOutcome);
        Assert.Equal(1, containsOutcome.Value);
        Assert.False(await ScriptExpression.RunAsync<int>("\"abc\" { \"AB\""));
        containsOutcome = await ScriptExpression.RunAsync<int>("\"abc\" { \"BC\"", ignoreCase);
        Assert.True(containsOutcome);
        Assert.Equal(1, containsOutcome.Value);
        containsOutcome = await ScriptExpression.RunAsync<int>("\"abc\" { \"abc\"");
        Assert.True(containsOutcome);
        Assert.Equal(0, containsOutcome.Value);
        Assert.True(await ScriptExpression.RunAsync("\"abc\" !{ \"ba\""));
        
        // contained ...
        containsOutcome = await ScriptExpression.RunAsync<int>("\"bc\" } \"abc\"");
        Assert.True(containsOutcome);
        Assert.Equal(1, containsOutcome.Value);
        Assert.False(await ScriptExpression.RunAsync("\"AB\" } \"abc\""));
        containsOutcome = await ScriptExpression.RunAsync<int>("\"BC\" } \"abc\"", ignoreCase);
        Assert.True(containsOutcome);
        Assert.Equal(1, containsOutcome.Value);
        containsOutcome = await ScriptExpression.RunAsync<int>("\"abc\" } \"abc\"");
        Assert.True(containsOutcome);
        Assert.Equal(0, containsOutcome.Value);
        Assert.True(await ScriptExpression.RunAsync("\"ba\" !} \"abc\""));
    }

    [Fact]
    public async Task Test_contained_operation()
    {
        var outcome = await ScriptExpression.RunAsync("17 } 42");
        Assert.False(outcome);
        Assert.Equal(Errors.CodeInvalidOperator, ((ScriptException)outcome.Exception!).ErrorCode);
        outcome = await ScriptExpression.RunAsync("42 { 17");
        Assert.Equal(Errors.CodeInvalidOperator, ((ScriptException)outcome.Exception!).ErrorCode);
        outcome = await ScriptExpression.RunAsync("42 { \"answer_is_42\"");
        Assert.Equal(Errors.CodeInvalidOperator, ((ScriptException)outcome.Exception!).ErrorCode);
        var outcomeResult = await ScriptExpression.RunAsync<int>("42 } \"answer_is_42\"");
        Assert.True(outcomeResult);
        Assert.Equal(10, outcomeResult.Value);
    }
}