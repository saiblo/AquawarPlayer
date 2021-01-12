using System;
using System.IO;
using GameHelper;
using LitJson;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine.UI;
using Utils;

public class Welcome : EnhancedMonoBehaviour
{
    public Transform[] fishPrefabSamples;

    public Sprite[] fishAvatars;

    public Transform offlineButton;
    public Transform onlineButton;
    public Transform exitButton;
    public InputField inputField;
    public Text inputPlaceholder;
    public Transform goButton;
    public Transform backButton;

    private bool _offline;

    private void Awake()
    {
        Screen.SetResolution(1920, 1080, true);
        for (var i = 0; i < Constants.FishNum; i++)
        {
            SharedRefs.FishPrefabs[i] = fishPrefabSamples[i];
            SharedRefs.FishAvatars[i] = fishAvatars[i];
        }
        SetIgb(false, true);
    }

    private void SetOoe(bool active)
    {
        offlineButton.gameObject.SetActive(active);
        onlineButton.gameObject.SetActive(active);
        exitButton.gameObject.SetActive(active);
    }

    private void SetIgb(bool active, bool offline)
    {
        if (active)
        {
            inputPlaceholder.text = offline ? "请输入回放文件路径..." : "请输入Token...";
            inputField.text = "";
            _offline = offline;
        }
        inputField.gameObject.SetActive(active);
        goButton.gameObject.SetActive(active);
        backButton.gameObject.SetActive(active);
    }

    public void OfflineButtonPressed()
    {
#if UNITY_EDITOR
        ProcessFile(EditorUtility.OpenFilePanel("选择回放文件", "", "json"));
#else
        SetOoe(false);
        SetIgb(true, true);
#endif
    }

    public void OnlineButtonPressed()
    {
        SetOoe(false);
        SetIgb(true, false);
    }

    public void BackButtonPressed()
    {
        SetIgb(false, false);
        SetOoe(true);
    }

    public void ConfirmButtonPressed()
    {
        if (_offline)
        {
            ProcessFile(inputField.text.Trim());
        }
    }

    private static void ProcessFile(string path)
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

    protected override void RunPerFrame()
    {
    }
}