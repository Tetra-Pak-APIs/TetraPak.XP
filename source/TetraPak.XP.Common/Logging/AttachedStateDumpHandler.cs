using System.Threading.Tasks;

namespace TetraPak.XP.Logging
{
    public delegate Task<bool> AttachedStateDumpHandler(object source, StateDumpContext context);
}