using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public void BubbleOnHover(Transform bubble)
    {
        bubble.localScale = new Vector3(6, 6, 6);
    }

    public void BubbleOnLoseHover(Transform bubble)
    {
        bubble.localScale = new Vector3(5, 5, 5);
    }
}