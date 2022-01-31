using System.Text;
using System.Threading.Tasks;

namespace TetraPak.Logging
{
    public delegate Task<bool> AttachedStateDumpHandler(object source, /*StringBuilder stringBuilder, obsolete */StateDumpContext context);
}