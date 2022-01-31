using System.Net;
using System.Net.Sockets;

namespace TetraPak.XP.Web
{
    /// <summary>
    ///   Provides network and network card related services.
    /// </summary>
    public class Network
    {
        /// <summary>
        ///   Gets an available (random) port for the local machine.
        /// </summary>
        /// <returns>
        ///   An port number (<see cref="int"/> value).
        /// </returns>
        public static int GetAvailablePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
        
    }
}