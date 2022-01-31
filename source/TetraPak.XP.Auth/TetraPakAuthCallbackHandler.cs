using System;
using System.Collections.Generic;

namespace TetraPak.XP.Auth
{
    /// <summary>
    ///   For internal use by platform code.
    /// </summary>
    public class TetraPakAuthCallbackHandler : IAuthCallbackHandler
    {
        readonly List<OnUriCallbackHandler> _handlers = new List<OnUriCallbackHandler>();

        void IAuthCallbackHandler.HandleUrlCallback(Uri uri)
        {
            var handlers = _handlers.ToArray();
            foreach (var handler in handlers)
            {
                try
                {
                    handler(uri, out var isHandled);
                    if (isHandled)
                        _handlers.Remove(handler);
                }
                catch
                {
                    // ignored
                }
            }
        }

        internal void NotifyUriCallback(OnUriCallbackHandler handler) => _handlers.Add(handler);
    }
}