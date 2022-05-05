using System.Threading.Tasks;

namespace TetraPak.XP.Mobile
{
    public interface IPlatformService
    {
        Task<Outcome> TryCloseTopWindowAsync(bool isModalWindow, bool animated = true);
    }
}