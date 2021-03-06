﻿using System;
using System.IO;
using System.Management.Automation;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace PSS.SupportModule
{
    public class ClientBundle : IDisposable
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _netStream;  // Raw-data stream of connection.
        private SslStream _ssl;            // Encrypts connection using SSL.
        private bool _disposed;
        public ClientMessageProcessor Processor { get; }
        public BinaryReader Reader { get; }
        public BinaryWriter Writer { get; }
        public ConnectSupportServicesCommand PssCmdlet { get; }

        public ClientBundle(ConnectSupportServicesCommand pssCmdlet, TcpClient client)
        {
            PssCmdlet = pssCmdlet;
            this._client = client;

            _netStream = client.GetStream();

            Authenticate();

            if (_ssl == null) return;

            Reader = new BinaryReader(_ssl, Encoding.UTF8);
            Writer = new BinaryWriter(_ssl, Encoding.UTF8);

            Processor = new ClientMessageProcessor(pssCmdlet, Writer);
        }

        private void Authenticate()
        {
            try
            {
                _ssl = new SslStream(_netStream, false, ValidateCert);
                _ssl.AuthenticateAsClient("PsSupportServer");
            }
            catch (Exception e)
            {
                for (var ex = e; ex != null; ex = ex.InnerException)
                {
                    PssCmdlet.WriteError(ex);
                }
            }
        }

        public bool IsConnected => _client?.Connected  ?? false;

        public bool Available => _netStream.DataAvailable;

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
                    _client?.Dispose();
                }
                catch
                {
                    // Ignore
                }
            }
        }

        private static bool ValidateCert(object sender, X509Certificate certificate,
            X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; // Allow untrusted certificates.
        }

    }
}