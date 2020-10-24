using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using LitJson;

public class Welcome : MonoBehaviour
{
    public Dialog openFileDialogPrefab;

    public Transform fishPrefabSample;

    private void Awake()
    {
        for (var i = 0; i < Constants.FishNum; i++)
            PrefabRefs.FishPrefabs[i] = fishPrefabSample;
    }

    public void OpenFile()
    {
        Instantiate(openFileDialogPrefab)
            .OpenFileDialog("打开文件", "~", ".json",
                (isSucessful, path) =>
                {
                    if (isSucessful)
                    {
                        try
                        {
                            using (var sr = new StreamReader(path))
                            {
                                string line;
                                var total = "";
                                while ((line = sr.ReadLine()) != null)
                                {
                                    total += line;
                                    total += "\n";
                                }
                                Debug.Log(total);
                                SceneManager.LoadScene("Scenes/Preparation");
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.Log(e);
                        }
                    }
                    else
                    {
                        Debug.Log("File not selected.");
                    }
                });
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    private class ConnectRequest
    {
        public string token { get; set; }
        public string request { get; set; }
    }

    public async void ConnectRoom(InputField tokenInputField)
    {
        var tokenEncoded = tokenInputField.text;
        var tokenDecoded = Encoding.UTF8.GetString(Convert.FromBase64String(tokenEncoded));
        if (tokenDecoded == tokenEncoded)
        {
            Debug.Log("Token解码失败");
            return;
        }
        try
        {
            Debug.Log("连接中……");
            var ws = new ClientWebSocket();
            await ws.ConnectAsync(new Uri($"wss://{tokenDecoded}"), CancellationToken.None);
            var request = new ConnectRequest {token = tokenEncoded, request = "connect"};
            await ws.SendAsync(
                new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonMapper.ToJson(request))),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
            Debug.Log("连接成功");
        }
        catch (Exception)
        {
            // ReSharper disable once Unity.InefficientPropertyAccess
            Debug.Log("连接失败");
        }
    }
}