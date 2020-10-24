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

    public void GoConnect()
    {
        SceneManager.LoadScene("Scenes/Connect");
    }
}