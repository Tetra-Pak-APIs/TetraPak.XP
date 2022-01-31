using System;
using System.Threading.Tasks;

namespace TetraPak.XP.Auth
{
    /// <summary>
    ///   For internal use by the Auth solution.
    /// </summary>
    public interface IAuthorizingAppDelegate
    {
        /// <summary>
        ///   For internal use by the Auth solution.
        /// </summary>
        void ActivateApp();

        /// <summary>
        ///   For internal use by the Auth solution.
        /// </summary>
        Task OpenInDefaultBrowserAsync(Uri uri);

        /// <summary>
        ///   For internal use by the Auth solution.
        /// </summary>
        Task OpenInDefaultBrowserAsync(Uri uri, Uri redirectUri);
    }
}
