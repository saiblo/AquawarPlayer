using System;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Connect : MonoBehaviour
{
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
            await Client.GameClient.Receive(); // BAN
            await Client.GameClient.Send(new Ok());
            await Client.GameClient.Receive(); // NOTICE
            await Client.GameClient.Send(new Ok());
            Debug.Log("连接成功");
            SceneManager.LoadScene("Scenes/Preparation");
        }
        catch (Exception)
        {
            Debug.Log("连接失败");
        }
    }
}