using System;

namespace TetraPak.XP.Browsers
{
    static class UriHelper
    {
        public static string AbsoluteHost(this Uri uri) => new UriBuilder(uri.Scheme, uri.Host, uri.Port).ToString();

        public static Uri EnsurePort(this Uri uri, int minimum = 0)
        {
            return uri.Port >= minimum 
                ? uri 
                : new UriBuilder(uri) { Port = LoopbackBrowser.GetRandomUnusedPort() }.Uri;
        }
    }
}