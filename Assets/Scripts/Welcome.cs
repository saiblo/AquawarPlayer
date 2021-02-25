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

    public Transform offlineButton;
    public Transform exitButton;
    public InputField inputField;
    public Transform goButton;
    public Transform backButton;

    public Text statusText;

    private void Awake()
    {
        Screen.SetResolution(1920, 1080, true);
        for (var i = 0; i < Constants.FishNum; i++)
        {
            SharedRefs.FishPrefabs[i] = fishPrefabSamples[i];
            SharedRefs.FishAvatars[i] = fishAvatars[i];
        }
        SetIgb(false);
    }

    private void SetOe(bool active)
    {
        offlineButton.gameObject.SetActive(active);
        exitButton.gameObject.SetActive(active);
    }

    private void SetIgb(bool active)
    {
        if (active) inputField.text = "";
        inputField.gameObject.SetActive(active);
        goButton.gameObject.SetActive(active);
        backButton.gameObject.SetActive(active);
    }

    public void OfflineButtonPressed()
    {
#if UNITY_EDITOR
        ProcessFile(EditorUtility.OpenFilePanel("选择回放文件", "", "json"));
#else
        SetOe(false);
        SetIgb(true);
#endif
    }

    public void BackButtonPressed()
    {
        SetIgb(false);
        SetOe(true);
    }

    public void ConfirmButtonPressed()
    {
        ProcessFile(inputField.text.Trim());
    }

    // ReSharper disable once MemberCanBeMadeStatic.Local
    private void ProcessFile(string path)
    {
        try
        {
            using (var reader = new StreamReader(path))
            {
                var replayJson = JsonMapper.ToObject(reader.ReadToEnd());
                if (Validators.ValidateJson(replayJson))
                {
                    SharedRefs.ReplayCursor = 0;
                    SharedRefs.ReplayJson = replayJson;
                    for (var i = 0; i < Constants.FishNum; i++)
                        SharedRefs.MyFishIdExpose[i] = SharedRefs.EnemyFishIdExpose[i] = false;
                    SceneManager.LoadScene("Scenes/Game");
                }
                else
                {
#if UNITY_EDITOR
                    EditorUtility.DisplayDialog("Error", "文件解析失败，请确认json文件格式是否正确。", "确认");
#else
                    statusText.text = "文件解析失败，请确认json文件格式是否正确。";
#endif
                }
            }
        }
        catch (Exception e)
        {
#if UNITY_EDITOR
            EditorUtility.DisplayDialog("Error", e.Message, "确认");
#else
            statusText.text = e.Message;
#endif
        }
    }
}