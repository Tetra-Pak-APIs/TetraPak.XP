using System;
using System.Threading.Tasks;

namespace TetraPak.XP.Scripting;

public interface IScriptContext
{
    bool IsDefaultContext { get; }
    
    StringComparison StringComparison { get; }
    
    Task<Outcome<object>> ResolveValueAsync(string key);
}