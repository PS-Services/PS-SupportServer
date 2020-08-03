using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using Newtonsoft.Json;

namespace PSS.SupportModule
{
    public class ClientMessageProcessor
    {
        public ConnectSupportServicesCommand PssCmdlet { get; }
        public BinaryWriter Writer { get; }

        public ConcurrentDictionary<string, Action<string>> Handlers { get; } =
            new ConcurrentDictionary<string, Action<string>>();

        public ClientMessageProcessor(ConnectSupportServicesCommand pssCmdlet, BinaryWriter writer)
        {
            PssCmdlet = pssCmdlet;
            Writer = writer;
        }

        public void ProcessMessage(string data)
        {
            try
            {
                PssCmdlet.WriteDebug(data);

                dynamic document = JsonConvert.DeserializeObject(data);

                string messageTypeString = document.MessageType;

                if (Handlers.TryGetValue(messageTypeString, out var handler))
                {
                    handler(data);
                    return;
                }

                handler = messageTypeString switch
                {
                    "Disconnect" => DisconnectHandler,
                    "Pong" => PongHandler,
                    "Ping" => PingHandler,
                    _ => UnknownMessage
                };

                handler?.Invoke(data);
            }
            catch(Exception ex)
            {
                PssCmdlet.WriteError(ex);
            }
        }

        private void DisconnectHandler(string json)
        {
            PssCmdlet.WriteVerbose("Handling Server Disconnect.");
            Disconnect?.Invoke(json);
        }

        private void PingHandler(string json)
        {

            var psObject = new InformationRecord(json, nameof(ClientMessageProcessor));

            PssCmdlet.WriteInformation(psObject);

            Writer.Write("{ \"MessageType\": \"Pong\" }");
            Writer.Flush();
        }

        private void PongHandler(string json)
        {
            PssCmdlet.WriteDebug("Got the PONG!");

            Pong?.Invoke(json);
        }

        public event Action<string> Pong;
        public event Action<string> Disconnect;

        private void UnknownMessage(string json) =>
            throw new ApplicationException($"Unknown Message: {json}");
    }
}