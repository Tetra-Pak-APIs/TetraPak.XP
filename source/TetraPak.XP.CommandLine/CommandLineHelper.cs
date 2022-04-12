using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace TetraPak.XP.CommandLine;

public static class CommandLineHelper
{
    public static bool TryGetNamedValue(
        this string[] args,
#if NET5_0_OR_GREATER     
        [NotNullWhen(true)]
#endif            
        out string? value,
        params string[] keys)
    {
        for (var i = 0; i < args.Length-1; i++)
        {
            var key = args[i];
            if ((keys.Length != 1 || keys[0] != key) && keys.All(item => item != key)) 
                continue;
            
            value = args[i + 1];
            return true;
        }

        value = null;
        return false;
    }
        
    public static bool TryGetNamedFlag(this string[] args, params string[] keys) 
        => 
        args.Any(key => keys.Any(i => i == key));
}