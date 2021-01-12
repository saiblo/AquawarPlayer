using System;
using System.IO;
using System.Text;
using LitJson;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
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
        
        string path = EditorUtility.OpenFilePanel("选择回放文件", "", "json");
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
                if (Validators.ValidateJson(replayJson))
                {
                    SharedRefs.ReplayCursor = 0;
                    SharedRefs.ReplayJson = replayJson;
                    SharedRefs.Mode = Constants.GameMode.Offline;
                    SceneManager.LoadScene("Scenes/Preparation");
                }
                else
                {
                    UnityEditor.EditorUtility.DisplayDialog("Error", "文件解析失败，请确认json文件格式是否正确。", "确认");
                    Debug.Log("文件解析失败");
                }
            }
        }
        catch (Exception e)
        {
            UnityEditor.EditorUtility.DisplayDialog("Error", e.Message.ToString(), "确认");
        }
    }

    public void GoConnect()
    {
        SceneManager.LoadScene("Scenes/Connect");
    }
}