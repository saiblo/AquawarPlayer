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
    public GameObject openFileDialogPrefab;

    public void OpenFile()
    {
        var openFileDialog = Instantiate(openFileDialogPrefab);
        var dialog = openFileDialog.GetComponent<Dialog>();
        dialog.OpenFileDialog("打开文件", "~", ".json", OnDialogComplete);
    }

    public void EnterGame()
    {
        SceneManager.LoadScene("Scenes/Preparation");
    }

    public async void ConnectRoom(InputField tokenInputField)
    {
        var tokenEncoded = tokenInputField.text;
        var tokenDecoded = Encoding.UTF8.GetString(Convert.FromBase64String(tokenEncoded));
        var ws = new ClientWebSocket();
        await ws.ConnectAsync(new Uri($"wss://{tokenDecoded}"), CancellationToken.None);
        await ws.SendAsync(
            new ArraySegment<byte>(Encoding.UTF8.GetBytes($"{{\"token\":\"{tokenEncoded}\",\"request\":\"connect\"}}")),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
        );
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

    private static void OnDialogComplete(bool isSucessful, string path)
    {
        var textGameObject = GameObject.Find("TextResult");
        if (isSucessful)
        {
            Debug.Log("Path : " + path);
            textGameObject.GetComponent<Text>().text = Show(path);
        }
        else
        {
            Debug.Log("No File/Folder Chosen, Cancel was pressed or something else happened.");
            textGameObject.GetComponent<Text>().text = "No File was selected. Press this button to try again.";
        }
    }
}