using System.Threading.Tasks;

namespace TetraPak.XP
{
    /// <summary>
    ///   Used to provide platform specific services.
    /// </summary>
    public interface IPlatformService
    {
        /// <summary>
        ///   Returns the current runtime platform.
        /// </summary>
        /// <seealso cref="RuntimePlatform"/>
        RuntimePlatform RuntimePlatform { get; }

        /// <summary>
        ///   Attempts closing the topmost window. 
        /// </summary>
        /// <param name="isModalWindow">
        ///   Specifies whether the top most window is modal.
        /// </param>
        /// <param name="animated">
        ///   (optional; default=true)<br/>
        ///   Attempts animating closing the window, when possible.
        /// </param>
        /// <returns></returns>
        Task<Outcome> TryCloseTopWindowAsync(bool isModalWindow, bool animated = true);
    }
}