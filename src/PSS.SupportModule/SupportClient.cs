using System;
using System.Management.Automation;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace PSS.SupportModule
{
    public class SupportClient
    {
        internal static SupportClient Current { get; private set; }

        private ConnectSupportServicesCommand PssCmdlet { get; }

        internal static SupportClient Create(ConnectSupportServicesCommand pssCmdlet) =>
            new SupportClient(pssCmdlet);

        private SupportClient(ConnectSupportServicesCommand pssCmdlet)
        {
            Current = this;
            PssCmdlet = pssCmdlet;
        }

        public void Connect(Uri uri)
        {
            if(uri.Scheme.ToLowerInvariant() != "pss") 
                throw new ArgumentException("Invalid URI.");

            var client = new TcpClient(uri.Host, uri.Port);

            Bundle = new ClientBundle(PssCmdlet, client);
            Bundle.Processor.Pong += PongHandler;
            Bundle.Processor.Disconnect += ServerDisconnected;

            PingTimer = new Timer(o => { });

            Task.Run(async () =>
            {
                PingTimer.Dispose();
                
                PingTimer = new Timer(Ping, null, TimeSpan.FromSeconds(20), TimeSpan.Zero);

                while (Bundle.IsConnected)
                {
                    if (Bundle.Available)
                    {
                        var message = Bundle.Reader.ReadString();
                        Bundle.Processor.ProcessMessage(message);
                    }

                    await Task.Delay(100);
                }

                PssCmdlet.WriteInformation(
                    new InformationRecord("Bundle is not connected.", nameof(SupportClient)));
            });
        }

        private void ServerDisconnected(string json)
        {
            PssCmdlet.WriteVerbose("Server Disconnected.");
            Bundle.Dispose();
        }

        private void PongHandler(string json)
        {
            PingTimer = new Timer(Ping, null, TimeSpan.FromSeconds(30), TimeSpan.Zero);
        }

        private void Ping(object? _)
        {
            Pinger(0);
        }

        private void Pinger(int count)
        {
            if (count == 3)
            {
                PssCmdlet.WriteVerbose("Ping Timeout.");
                CloseConn();
                return;
            }

            try
            {
                PingTimer.Dispose();

                PssCmdlet.WriteDebug($"Ping Retry {count + 1}.");

                Bundle.Writer.Write(@"{ ""MessageType"": ""Ping"" }");

                Bundle.Writer.Flush();

                PongTimer = new Timer(o =>
                {
                    Pinger(count++);
                }, null, TimeSpan.FromSeconds(10), TimeSpan.Zero);
            }
            catch
            {
                try
                {
                    CloseConn();
                }
                catch
                {
                    // Ignore;
                }
            }
        }

        public Timer PongTimer { get; set; }

        public Timer PingTimer { get; set; }

        public ClientBundle Bundle { get; set; }

        public void Disconnect()
        {
            if (!(Bundle?.IsConnected ?? false)) return;

            Bundle.Writer.Write(@"{ ""MessageType"": ""Disconnect"" }");
            Bundle.Writer.Flush();

            CloseConn();
        }

        private void CloseConn()
        {
            PssCmdlet.WriteVerbose("Connection closing.");

            Bundle.Dispose();
            PingTimer?.Dispose();
            PongTimer?.Dispose();
        }
    }
}