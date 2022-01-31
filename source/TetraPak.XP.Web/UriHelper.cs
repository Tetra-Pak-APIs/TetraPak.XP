using System;

namespace TetraPak.XP.Web
{
    public static class UriHelper
    {
        public static string AbsoluteHost(this Uri uri) => new UriBuilder(uri.Scheme, uri.Host, uri.Port).ToString();

        /// <summary>
        ///   Examines a <see cref="Uri"/> and ensures it has a specified port element, adding it if necessary.
        /// </summary>
        /// <param name="uri">
        ///   The <see cref="Uri"/> to be examined.
        /// </param>
        /// <param name="minimum">
        ///   A minimum port number to look for. If the <paramref name="uri"/>'s assigned port is less
        ///   than the specified minimum a random port will be used in the resulting <see cref="Uri"/>.  
        /// </param>
        /// <returns>
        ///   A <see cref="Uri"/> that is guaranteed to have a port element.
        /// </returns>
        public static Uri EnsurePort(this Uri uri, int minimum = 0)
        {
            return uri.Port >= minimum 
                ? uri 
                : new UriBuilder(uri) { Port = Network.GetAvailablePort() }.Uri;
        }
    }
}