using System;

namespace TetraPak.XP.Auth
{
    /// <summary>
    ///   For internal use by platform code.
    /// </summary>
    public delegate void OnUriCallbackHandler(Uri uri, out bool isHandled);
}