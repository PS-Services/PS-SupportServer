using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PSS.SupportServer
{
    internal class Server
    {
        private readonly IPAddress _ip = IPAddress.Parse("0.0.0.0");
        private const int Port = 2000;
        private TcpListener? _server;

        private readonly ConcurrentBag<ClientBundle> _clients = 
            new ConcurrentBag<ClientBundle>();

        private CancellationTokenSource _tokenSource;

        public void Listen()
        {
            _tokenSource = new CancellationTokenSource();

            _server = new TcpListener(_ip, Port);
            _server.Start();

            Task.Run(() =>
            {
                while (!_tokenSource.IsCancellationRequested)
                {
                    var tcpClient = _server.AcceptTcpClient();

                    var bundle = new ClientBundle(tcpClient, token: _tokenSource.Token);

                    var handler = new ClientHandler(bundle);

                    var thread = new Thread(handler.HandleClient);

                    _clients.Add(bundle);

                    thread.Start();
                }
            });
        }

        public void Shutdown()
        {
            foreach (var bundle in _clients)
            {
                bundle?.SendDisconnect();
            }

            _tokenSource.Cancel(false);
        }
    }
}