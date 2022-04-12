using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace TetraPak.XP.Scripting.Tests;

public sealed class TestContextValues
{
    [Fact]
    public async Task Nisse()
    {
        ScriptValue.AddParsers(new NamedValueParser());
        var context = new ScriptContext();
        var obj = new List<object>();
        obj.AddRange( new[]  {"Hello", "World"});
        await context.Values.SetValueAsync("obj", obj);
        Assert.True(await ScriptExpression.RunAsync<bool>("obj[0] == \"Hello\"", context));
    }
}