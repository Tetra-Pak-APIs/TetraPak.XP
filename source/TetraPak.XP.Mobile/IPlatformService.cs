using System.Threading.Tasks;

namespace TetraPak.XP.Mobile
{
    public interface IPlatformService
    {
        Task CloseTopWindowAsync(bool isModalWindow, bool animated = true);
    }
}