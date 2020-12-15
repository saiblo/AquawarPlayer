using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
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
            await Client.GameClient.Send();
            Debug.Log(JsonMapper.ToJson(await Client.GameClient.Receive()));
            await Client.GameClient.Send(new Ok());
            Debug.Log(JsonMapper.ToJson(await Client.GameClient.Receive()));
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