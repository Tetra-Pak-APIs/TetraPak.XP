using System.Threading.Tasks;
using TetraPak.XP.Diagnostics;

namespace TetraPak.XP.Logging
{
    public delegate Task<bool> AttachedStateDumpHandler(object source, StateDumpContext context);
}