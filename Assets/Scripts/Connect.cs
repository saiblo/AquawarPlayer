using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using LitJson;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Connect : MonoBehaviour
{
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
            Client.GameClient = new Client(tokenDecoded, tokenEncoded);
            await Client.GameClient.Send("connect");
            Debug.Log("连接成功");
            SceneManager.LoadScene("Scenes/Preparation");
        }
        catch (Exception)
        {
            // ReSharper disable once Unity.InefficientPropertyAccess
            Debug.Log("连接失败");
        }
    }
}