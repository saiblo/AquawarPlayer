using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LitJson;
using UnityEngine;

public class Client
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    private class Request
    {
        public string token { get; set; }
        public string request { get; set; }
        public string content { get; set; }
    }

    private readonly ClientWebSocket _ws;

    private readonly string _token;

    public Client(string tokenDecoded, string tokenEncoded)
    {
        _ws = new ClientWebSocket();
        _ws.ConnectAsync(new Uri($"wss://{tokenDecoded}"), CancellationToken.None).Wait();
        _token = tokenEncoded;
    }

    public Task Send(string content = null)
    {
        return _ws.SendAsync(
            new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonMapper.ToJson(
                new Request {token = _token, request = content == null ? "connect" : "action", content = content ?? ""}
            ))),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
        );
    }

    public async Task<string> Receive()
    {
        var buffer = new ArraySegment<byte>(new byte[1024]);
        await _ws.ReceiveAsync(buffer, CancellationToken.None);
        return (string) JsonMapper.ToObject(Encoding.UTF8.GetString(buffer.Array ?? Array.Empty<byte>()))["content"];
    }

    public static Client GameClient = null;
}