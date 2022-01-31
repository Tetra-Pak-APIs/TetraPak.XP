using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TetraPak.Auth.Xamarin.common;

namespace TetraPak.XP.UWP
{
    public class LocalHost
    {
        Task _task;
        readonly string[] _prefixes;
        readonly Uri _indexUri;
        readonly TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();

        public static int DefaultPort { get; set; } = 8080;

        public static int FirstFreePort => getRandomUnusedPort();

        public static string LocalAddress
        {
            get
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }

                throw new Exception("No network adapters with an IPv4 address in the system!");
            }
        }

        public static string LocalhostUri(int port = 0)
        {
            if (port <= 0)
                port = FirstFreePort;

            return $"http://localhost:{port}";
        }

        static int getRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        void handleRequests()
        {
            var listener = new HttpListener { AuthenticationSchemes = AuthenticationSchemes.Anonymous };

            foreach (var prefix in _prefixes)
            {
                listener.Prefixes.Add(prefix.EnsureEndsWith("/"));
            }

            listener.Start();
            while (listener.IsListening)
            {
                try
                {
                    var result = listener.BeginGetContext(onHandleRequest, listener);
                    if (_tcs.Task.Status == TaskStatus.WaitingForActivation)
                        _tcs.SetResult(true);

                    result.AsyncWaitHandle.WaitOne();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    _tcs.SetResult(false);
                    throw;
                }
            }
            listener.Stop();
        }

        void onHandleRequest(IAsyncResult ar)
        {
            var listener = (HttpListener)ar.AsyncState;
            var context = listener.EndGetContext(ar);
            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";

            var content = "<HTML><BODY>Hello world!</BODY></HTML>";
            var buffer = Encoding.UTF8.GetBytes(content);
            context.Response.ContentType = "text/html";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.ContentLength64 = buffer.Length;
            var stream = context.Response.OutputStream;
            stream.Write(buffer, 0, buffer.Length);
            context.Response.Close();
        }

        public Task<bool> RunAsync()
        {
            if (_task != null)
                throw new InvalidOperationException($"Cannot start {GetType()} a second time.");

            _task = Task.Run(handleRequests);
            return _tcs.Task;
        }

        public LocalHost(Uri indexUri, params string[] prefixes)
        {
            if (indexUri == null) throw new ArgumentNullException(nameof(indexUri));
            if (prefixes.Length == 0) prefixes = new[]
            {
                $"http://localhost:{DefaultPort}/",
                $"http://{LocalAddress}:{DefaultPort}"
            };
            if (!HttpListener.IsSupported)
                throw new Exception($"LocalHost cannot initiate because the APIs needed ({typeof(HttpListener)}) is not supported.");

            _indexUri = indexUri;
            _prefixes = prefixes;
        }
    }
}