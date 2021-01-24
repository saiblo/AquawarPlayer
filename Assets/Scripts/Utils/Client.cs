using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LitJson;
using UnityEngine;

namespace Utils
{
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

        public GameUI GameUI;

        public Client(string tokenDecoded, string tokenEncoded, bool local)
        {
            _ws = new ClientWebSocket();
            _ws.ConnectAsync(new Uri($"{(local ? "ws" : "wss")}://{tokenDecoded}"), CancellationToken.None).Wait();
            _token = tokenEncoded;
        }

        public async Task Send(object content = null)
        {
            try
            {
                if (content != null) Debug.Log($"Send: {JsonMapper.ToJson(content)}");
                await _ws.SendAsync(
                    new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonMapper.ToJson(
                        new Request
                        {
                            token = _token,
                            request = content == null ? "connect" : "action",
                            content = content == null ? "" : JsonMapper.ToJson(content)
                        }
                    ))),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None
                );
            }
            catch (Exception e)
            {
                if (GameUI) GameUI.resultText.text = $"网络异常，游戏结束。\n{e.Message}";
                GameUI.gameOverMask.SetActive(true);
                throw;
            }
        }

        public async Task<JsonData> Receive()
        {
            try
            {
                var buffer = new ArraySegment<byte>(new byte[32768]);
                await _ws.ReceiveAsync(buffer, CancellationToken.None);
                var str = Encoding.UTF8.GetString(buffer.Array ?? Array.Empty<byte>()).TrimEnd('\0');
                if (str.Trim().Length == 0) throw new Exception("The client did not receive anything from the server.");
                var data = (string) JsonMapper.ToObject(str)["content"];
                Debug.Log($"Recv: {JsonMapper.ToJson(JsonMapper.ToObject(data))}");
                return JsonMapper.ToObject(data);
            }
            catch (Exception e)
            {
                if (GameUI) GameUI.resultText.text = $"网络异常，游戏结束。\n{e.Message}";
                GameUI.gameOverMask.SetActive(true);
                SharedRefs.ErrorFlag = true;
                throw;
            }
        }
    }
}