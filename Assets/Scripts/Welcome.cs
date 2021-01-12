using System;
using System.IO;
using LitJson;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine.UI;
using Utils;

public class Welcome : MonoBehaviour
{
    public Transform[] fishPrefabSamples;

    public Sprite[] fishAvatars;

    public InputField pathInputField;

    private void Awake()
    {
        Screen.SetResolution(1920, 1080, true);
        for (var i = 0; i < Constants.FishNum; i++)
        {
            SharedRefs.FishPrefabs[i] = fishPrefabSamples[i];
            SharedRefs.FishAvatars[i] = fishAvatars[i];
        }
    }

    public void OpenFile()
    {
#if UNITY_EDITOR
        var path = pathInputField.text.Trim() == ""
            ? EditorUtility.OpenFilePanel("选择回放文件", "", "json")
            : pathInputField.text.Trim();
#else
        var path = pathInputField.text.Trim();
#endif
        try
        {
            using (var reader = new StreamReader(path))
            {
                var replayJson = JsonMapper.ToObject(reader.ReadToEnd());
                if (Validators.ValidateJson(replayJson))
                {
                    SharedRefs.ReplayCursor = 0;
                    SharedRefs.ReplayJson = replayJson;
                    SharedRefs.Mode = Constants.GameMode.Offline;
                    SceneManager.LoadScene("Scenes/Preparation");
                }
                else
                {
#if UNITY_EDITOR
                    EditorUtility.DisplayDialog("Error", "文件解析失败，请确认json文件格式是否正确。", "确认");
#endif
                }
            }
        }
        catch (Exception e)
        {
#if UNITY_EDITOR
            EditorUtility.DisplayDialog("Error", e.Message, "确认");
#endif
        }
    }

    public void GoConnect()
    {
        SceneManager.LoadScene("Scenes/Connect");
    }
}