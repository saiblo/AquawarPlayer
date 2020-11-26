using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LitJson;

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

    public Task Send(string request, string content = "")
    {
        return _ws.SendAsync(
            new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonMapper.ToJson(
                new Request {token = _token, request = request, content = content}
            ))),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
        );
    }

    public static Client GameClient = null;
}