using System.Threading.Tasks;

namespace TetraPak.XP.Diagnostics
{
    public delegate Task<bool> AttachedStateDumpHandler(object source, StateDumpContext context);
}