using System;
using System.Management.Automation;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace PSS.SupportModule
{
    public class SupportClient
    {
        private ConnectSupportServicesCommand PssCmdlet { get; }

        public SupportClient(ConnectSupportServicesCommand pssCmdlet)
        {
            PssCmdlet = pssCmdlet;
        }

        public void Connect(Uri uri)
        {
            if(uri.Scheme.ToLowerInvariant() != "pss") 
                throw new ArgumentException("Invalid URI.");

            var client = new TcpClient(uri.Host, uri.Port);

            Bundle = new ClientBundle(PssCmdlet, client);

            Task.Run(async () =>
            {
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

        public ClientBundle Bundle { get; set; }

        public void Disconnect()
        {
            if (!(Bundle?.IsConnected ?? false)) return;

            CloseConn();
        }

        private void CloseConn()
        {
            Bundle.Dispose();
        }
    }
}