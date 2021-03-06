﻿using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace PSS.SupportServer
{
    public class ServerMessageProcessor
    {
        public BinaryWriter Writer { get; }
        public CancellationToken Token { get; }

        public ConcurrentDictionary<string, Action<JsonDocument>> Handlers { get; } =
            new ConcurrentDictionary<string, Action<JsonDocument>>();

        public ServerMessageProcessor(
            BinaryWriter writer, 
            CancellationToken token)
        {
            Writer = writer;
            Token = token;
        }

        public void ProcessMessage(string data)
        {
            if (Token.IsCancellationRequested) return;

            var document = JsonDocument.Parse(data, new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            });

            var messageTypeString = document.RootElement.GetProperty("MessageType").GetString();

            if (Handlers.TryGetValue(
                messageTypeString ?? throw new InvalidOperationException(), 
                out var handler))
            {
                handler(document);
                return;
            }

            handler = messageTypeString switch
            {
                "Disconnect" => DisconnectHandler,
                "Pong" => PongHandler,
                "Ping" => PingHandler,
                _ => UnknownMessage
            };

            handler?.Invoke(document);
        }

        private void DisconnectHandler(JsonDocument document)
        {
            Disconnect?.Invoke(document);
        }

        private void PingHandler(JsonDocument document)
        {
            Writer.Write("{ \"MessageType\": \"Pong\" }");
            Writer.Flush();
        }

        private void PongHandler(JsonDocument document)
        {
            if (Token.IsCancellationRequested) return;

            Console.WriteLine("Got the PONG!");

            Pong?.Invoke(document);
        }

        public event Action<JsonDocument> Pong;
        public event Action<JsonDocument> Disconnect;

        private static void UnknownMessage(JsonDocument document) =>
            throw new ApplicationException($"Unknown Message: {document}");
    }
}