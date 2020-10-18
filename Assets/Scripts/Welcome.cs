using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Welcome : MonoBehaviour
{
    public Dialog openFileDialogPrefab;

    public Text textResult;

    public void OpenFile()
    {
        Instantiate(openFileDialogPrefab)
            .OpenFileDialog("打开文件", "~", ".json",
                (isSucessful, path) => { textResult.text = isSucessful ? Show(path) : "未选择任何文件"; });
    }

    public void EnterGame()
    {
        SceneManager.LoadScene("Scenes/Preparation");
    }

    public async void ConnectRoom(InputField tokenInputField)
    {
        var tokenEncoded = tokenInputField.text;
        var tokenDecoded = Encoding.UTF8.GetString(Convert.FromBase64String(tokenEncoded));
        if (tokenDecoded == tokenEncoded)
        {
            textResult.text = "Token解码失败";
            return;
        }
        try
        {
            textResult.text = "连接中……";
            var ws = new ClientWebSocket();
            await ws.ConnectAsync(new Uri($"wss://{tokenDecoded}"), CancellationToken.None);
            await ws.SendAsync(
                new ArraySegment<byte>(
                    Encoding.UTF8.GetBytes($"{{\"token\":\"{tokenEncoded}\",\"request\":\"connect\"}}")),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
            textResult.text = "连接成功";
        }
        catch (Exception)
        {
            // ReSharper disable once Unity.InefficientPropertyAccess
            textResult.text = "连接失败";
        }
    }

    private static string Show(string path)
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
                return total;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("The file could not be read:");
            Console.WriteLine(e.Message);
            return $"The file could not be read: {path}.";
        }
    }
}