using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PSS.SupportServer
{
    internal class ClientHandler : IDisposable
    {
        public ClientHandler(ClientBundle bundle)
        {
            Bundle = bundle;
            Bundle.Processor.Pong += PongHandler;
            Bundle.Processor.Disconnect += ClientDisconnect;
            PingTimer = new Timer(o => { });
        }

        private void ClientDisconnect(JsonDocument obj)
        {
            Bundle.Dispose();
        }

        private void PongHandler(System.Text.Json.JsonDocument obj)
        {
            PingTimer = new Timer(Ping, null, TimeSpan.FromSeconds(30), TimeSpan.Zero);
        }

        public async void HandleClient()
        {
            Console.WriteLine($"Accepted connection from: {Bundle.Address}");

            await PingTimer.DisposeAsync();
            PingTimer = new Timer(Ping, null, TimeSpan.FromSeconds(30), TimeSpan.Zero);

            while (Bundle.IsConnected && !Bundle.Token.IsCancellationRequested)
            {
                try
                {
                    if (Bundle.Available)
                    {
                        var message = Bundle.Reader.ReadString();
                        Bundle.Processor.ProcessMessage(message);
                    }
                }
                catch
                {
                    break;
                }

                await Task.Delay(150);
            }

            if (Bundle.IsConnected)
            {
                Bundle.SendDisconnect();
            }
            else
            {
                Console.WriteLine($"Lost connection from {Bundle.Address}");
            }

            Dispose();
        }

        private Timer PingTimer { get; set; }

        private ClientBundle Bundle { get; }

        private void Ping(object? _)
        {
            Pinger(0);
        }

        private void Pinger(int count)
        {
            if (count == 3)
            {
                Console.WriteLine("Ping Timeout!");
                Dispose();
                return;
            }

            try
            {
                PingTimer.Dispose();

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
                    Dispose();
                }
                catch
                {
                    // Ignore;
                }
            }
        }

        public Timer PongTimer { get; set; }

        public void Dispose()
        {
            try
            {
                PingTimer.Dispose();
            }
            catch
            {
                // Ignore
            }

            try
            {
                PongTimer?.Dispose();
            }
            catch
            {
                // Ignore
            }


            try
            {
                Bundle.Dispose();
            }
            catch
            {
                // Ignore
            }
        }
    }
}