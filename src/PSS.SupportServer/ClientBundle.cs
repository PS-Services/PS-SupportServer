using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace PSS.SupportServer
{
    public class ClientBundle : IDisposable
    {
        private readonly X509Certificate2 _cert = 
            new X509Certificate2("server.pfx", "password");

        private readonly NetworkStream _netStream;  // Raw-data stream of connection.
        private readonly SslStream _ssl;            // Encrypts connection using SSL.
        private bool _disposed;
        private string _address;
        public TcpClient Client { get; }
        public ServerMessageProcessor Processor { get; }
        public BinaryReader Reader { get; }
        public BinaryWriter Writer { get; }
        public CancellationToken Token { get; }

        public ClientBundle(
            TcpClient client, 
            CancellationToken token, 
            string? certFile = null,
            string? certPassword = null)
        {
            Token = token;
            if (certFile != null)
            {
                _cert = new X509Certificate2(certFile, certPassword);
            }

            this.Client = client;
            _netStream = client.GetStream();
            _ssl = new SslStream(_netStream, false);
            _ssl.AuthenticateAsServer(_cert, false, 
                SslProtocols.Tls12, true);

            Reader = new BinaryReader(_ssl, Encoding.UTF8);
            Writer = new BinaryWriter(_ssl, Encoding.UTF8);

            Processor = new ServerMessageProcessor(Writer, Token);
        }
        internal void SendDisconnect()
        {
            Writer.Write("{ \"MessageType\": \"Disconnect\" }");
            Writer.Flush();

            Dispose();
        }

        public bool IsConnected => Client?.Connected ?? false;

        public bool Available => _netStream.DataAvailable;
        public string Address => _address ??=
            Client.Client.RemoteEndPoint?.Serialize().ToString() ?? "Unknown";

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                try
                {
                    Reader?.Dispose();
                }
                catch
                {
                    // Ignore
                }
                try
                {
                    Writer?.Dispose();
                }
                catch
                {
                    // Ignore
                }
                try
                {
                    _ssl?.Dispose();
                }
                catch
                {
                    // Ignore
                }
                try
                {
                    _netStream?.Dispose();
                }
                catch
                {
                    // Ignore
                }

                try
                {
                    Client?.Dispose();
                }
                catch
                {
                    // Ignore
                }
            }
        }
    }
}