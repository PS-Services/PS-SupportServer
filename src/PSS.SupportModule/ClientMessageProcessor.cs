using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text.Json;

namespace PSS.SupportModule
{
    public class ClientMessageProcessor
    {
        public ConnectSupportServicesCommand PssCmdlet { get; }
        public BinaryWriter Writer { get; }

        public ConcurrentDictionary<string, Action<JsonDocument>> Handlers { get; } =
            new ConcurrentDictionary<string, Action<JsonDocument>>();

        public ClientMessageProcessor(ConnectSupportServicesCommand pssCmdlet, BinaryWriter writer)
        {
            PssCmdlet = pssCmdlet;
            Writer = writer;
        }

        public void ProcessMessage(string data)
        {
            try
            {
                PssCmdlet.WriteVerbose(data);

                var document = JsonDocument.Parse(data, new JsonDocumentOptions
                {
                    AllowTrailingCommas = true,
                    CommentHandling = JsonCommentHandling.Skip
                });

                var messageTypeString = document.RootElement.GetProperty("MessageType").GetString();

                if (Handlers.TryGetValue(messageTypeString, out var handler))
                {
                    handler(document);
                    return;
                }

                handler = messageTypeString switch
                {
                    "Ping" => PingHandler,
                    _ => UnknownMessage
                };

                handler?.Invoke(document);
            }
            catch(Exception ex)
            {
                PssCmdlet.WriteError(ex);
            }
        }

        private void PingHandler(JsonDocument document)
        {
            var ms = new MemoryStream();
            var jsonWriter = new Utf8JsonWriter(ms);
            document.WriteTo(jsonWriter);
            jsonWriter.Flush();
            ms.Seek(0, SeekOrigin.Begin);

            var json = new StreamReader(ms).ReadToEnd();

            var psObject = new InformationRecord(json, nameof(ClientMessageProcessor));

            PssCmdlet.WriteInformation(psObject);

            Writer.Write("{ \"MessageType\": \"Pong\" }");
            Writer.Flush();
        }

        private void UnknownMessage(JsonDocument document) =>
            throw new ApplicationException($"Unknown Message: {document}");
    }
}