using System;
using System.Globalization;
using System.IO;
using System.Text;
using LitJson;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Welcome : MonoBehaviour
{
    public Dialog openFileDialogPrefab;

    public Transform fishPrefabSample;
    public Transform fishPrefabSample2;
    public Transform fishPrefabSample3;
    public Transform fishPrefabSample4;
    public Transform fishPrefabSample5;
    public Transform fishPrefabSample6;

    public Transform bubbleOpenFile;

    public Transform bubbleConnect;

    private Vector3 _bubbleOpenFileOriginalPos;

    private Vector3 _bubbleConnectOriginalPos;

    private DateTime _initialTime;

    private void Awake()
    {
        for (var i = 0; i < Constants.FishNum / 6; i++)
        {
            PrefabRefs.FishPrefabs[i * 6] = fishPrefabSample;
            PrefabRefs.FishPrefabs[i * 6 + 1] = fishPrefabSample2;
            PrefabRefs.FishPrefabs[i * 6 + 2] = fishPrefabSample3;
            PrefabRefs.FishPrefabs[i * 6 + 3] = fishPrefabSample4;
            PrefabRefs.FishPrefabs[i * 6 + 4] = fishPrefabSample5;
            PrefabRefs.FishPrefabs[i * 6 + 5] = fishPrefabSample6;
        }
        _bubbleOpenFileOriginalPos = bubbleOpenFile.localPosition;
        _bubbleConnectOriginalPos = bubbleConnect.localPosition;
        _initialTime = DateTime.Now;
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
                                var replaySb = new StringBuilder();
                                while ((line = sr.ReadLine()) != null)
                                {
                                    replaySb.Append(line);
                                    replaySb.Append('\n');
                                }
                                var replay = replaySb.ToString();
                                if (JsonMapper.ToObject(replay).IsArray)
                                {
                                    PlayerPrefs.SetString("replay", replay);
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

    public void BubbleOnHover(Transform bubble)
    {
        bubble.localScale = new Vector3(6, 6, 6);
    }

    public void BubbleOnLoseHover(Transform bubble)
    {
        bubble.localScale = new Vector3(5, 5, 5);
    }

    private static float GetNewPos(float original, TimeSpan timeSpan, int omega, float a)
    {
        return float.Parse(
            (original + a * Math.Sin(timeSpan.TotalMilliseconds / Convert.ToDouble(omega))).ToString(
                CultureInfo.InvariantCulture));
    }

    private void Update()
    {
        var timeSpan = DateTime.Now - _initialTime;
        bubbleOpenFile.localPosition = new Vector3(
            GetNewPos(_bubbleOpenFileOriginalPos.x, timeSpan, 1500, 0.8f),
            GetNewPos(_bubbleOpenFileOriginalPos.y, timeSpan, 2048, 0.6f),
            GetNewPos(_bubbleOpenFileOriginalPos.z, timeSpan, 320, 0.1f)
        );
        bubbleConnect.localPosition = new Vector3(
            GetNewPos(_bubbleConnectOriginalPos.x, timeSpan, 1280, -0.8f),
            GetNewPos(_bubbleConnectOriginalPos.y, timeSpan, 2333, 0.6f),
            GetNewPos(_bubbleConnectOriginalPos.z, timeSpan, 361, 0.1f)
        );
    }
}