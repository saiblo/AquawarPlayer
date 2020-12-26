using System;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Connect : MonoBehaviour
{
    public Text statusText;

    public async void ConnectRoom(InputField tokenInputField)
    {
        var tokenEncoded = tokenInputField.text;
        var tokenDecoded = Encoding.UTF8.GetString(Convert.FromBase64String(tokenEncoded));
        if (tokenDecoded == tokenEncoded)
        {
            statusText.text = "Token解码失败";
            return;
        }
        try
        {
            statusText.text = "连接中……";
            Client.GameClient = new Client(tokenDecoded, tokenEncoded);
            await Client.GameClient.Send();
            statusText.text = "连接成功，等待对手中……";
            await Client.GameClient.Receive(); // NOTICE
            await Client.GameClient.Send(new Ok());
            SharedRefs.Mode = Constants.GameMode.Online;
            SceneManager.LoadScene("Scenes/Preparation");
        }
        catch (Exception)
        {
            statusText.text = "连接失败";
        }
    }
}