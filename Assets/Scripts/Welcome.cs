using System;
using System.IO;
using System.Text;
using LitJson;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

public class Welcome : MonoBehaviour
{
    public Dialog openFileDialogPrefab;

    public Transform[] fishPrefabSamples;

    private void Awake()
    {
        Screen.SetResolution(1920, 1080, true);
        for (var i = 0; i < Constants.FishNum; i++)
            SharedRefs.FishPrefabs[i] = fishPrefabSamples[i];
    }

    public void OpenFile()
    {
        Instantiate(openFileDialogPrefab)
            .OpenFileDialog("打开文件", "~", ".json",
                (isSuccessful, path) =>
                {
                    if (isSuccessful)
                    {
                        try
                        {
                            using (var sr = new StreamReader(path))
                            {
                                string line;
                                var replaySb = new StringBuilder();
                                while ((line = sr.ReadLine()) != null)
                                {
                                    replaySb.Append(line);
                                    replaySb.Append('\n');
                                }
                                var replayJson = JsonMapper.ToObject(replaySb.ToString());
                                if (replayJson.IsArray)
                                {
                                    SharedRefs.ReplayCursor = 0;
                                    SharedRefs.ReplayJson = replayJson;
                                    SharedRefs.Mode = Constants.GameMode.Offline;
                                    SceneManager.LoadScene("Scenes/Preparation");
                                }
                                else
                                {
                                    Debug.Log("文件解析失败");
                                }
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