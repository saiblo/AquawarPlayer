﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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

    public Text statusText;

    [DllImport("__Internal")]
    private static extern string GetReplay();

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
        if (_offline) ProcessFile(inputField.text.Trim());
        else ConnectRoom(inputField.text.Trim());
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
                    SharedRefs.Mode = Constants.GameMode.Offline;
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

    private async void ConnectRoom(string tokenEncoded)
    {
        var tokenDecoded = tokenEncoded;
        try
        {
            tokenDecoded = Encoding.UTF8.GetString(Convert.FromBase64String(tokenEncoded));
        }
        catch (Exception)
        {
            // ignored
        }
        var local = tokenDecoded == tokenEncoded;
        if (local) tokenEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(tokenDecoded));
        try
        {
            statusText.text = "连接中……";
            SharedRefs.GameClient = new Client(tokenDecoded, tokenEncoded, local);
            await SharedRefs.GameClient.Send();
            statusText.text = "连接成功，等待对手中……";
            new Thread(() =>
            {
                try
                {
                    SharedRefs.GameClient.RecvHandle.WaitOne();
                    SharedRefs.PickInfo = SharedRefs.GameClient.RecvBuffer; // PICK
                    SharedRefs.Mode = Constants.GameMode.Online;
                    SharedRefs.OnlineWin = SharedRefs.OnlineLose = 0;
                    RunOnUiThread(() => { SceneManager.LoadScene("Scenes/Preparation"); });
                }
                catch (Exception)
                {
                    RunOnUiThread(() => { statusText.text = "连接失败"; });
                }
            }).Start();
        }
        catch (Exception)
        {
            statusText.text = "连接失败";
        }
    }

    protected override void RunPerFrame()
    {
        var replay = GetReplay();
        Console.WriteLine("Get replay: " + replay);
        if (replay.Length <= 0) return;

        try
        {
            var replayJson = JsonMapper.ToObject(replay);
            if (Validators.ValidateJson(replayJson))
            {
                SharedRefs.ReplayCursor = 0;
                SharedRefs.ReplayJson = replayJson;
                SharedRefs.Mode = Constants.GameMode.Offline;
                SceneManager.LoadScene("Scenes/Game");
            }
            else
            {
                statusText.text = "文件解析失败，请确认json文件格式是否正确。";
            }
        }
        catch (Exception e)
        {
            statusText.text = e.Message;
        }
    }
}