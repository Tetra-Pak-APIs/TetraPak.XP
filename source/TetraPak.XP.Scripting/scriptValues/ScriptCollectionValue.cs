using System.Threading.Tasks;

namespace TetraPak.XP.Scripting;

public abstract class ScriptCollectionValue : ScriptValue
{
    public abstract Task<Outcome<int>> Contains(ScriptValue value);

    public abstract Task<Outcome> NotContains(ScriptValue value);

    protected ScriptCollectionValue(string stringValue) : base(stringValue)
    {
    }
}