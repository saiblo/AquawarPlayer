using System;
using System.Globalization;
using System.IO;
using System.Text;
using LitJson;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;

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

    public Transform wanderingBubble;

    private Vector3 _bubbleOpenFileOriginalPos;

    private Vector3 _bubbleConnectOriginalPos;

    private DateTime _initialTime;

    private int _wanderingBubbleRound = -1;

    private const int WanderingBubbleInterval = 15000;

    private readonly Random _random = new Random(Convert.ToInt32(DateTime.Now.Ticks % 100000000));

    private float _currRandX;

    private double _timeBias;

    private void Awake()
    {
        PlayerPrefs.SetString("replay", "");
        PlayerPrefs.SetInt("cursor", 0);
        Screen.SetResolution(1920, 1080, true);
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
                                    PlayerPrefs.SetInt("cursor", 0);
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

    public void WanderingBubbleOnHover()
    {
        wanderingBubble.localScale = new Vector3(3, 3, 3);
    }

    public void WanderingBubbleOnLoseHover()
    {
        wanderingBubble.localScale = new Vector3(2, 2, 2);
    }

    public void WanderingBubbleOnClick()
    {
        wanderingBubble.localScale = new Vector3(2, 2, 2);
        _wanderingBubbleRound++;
        _currRandX = _random.Next(-54, 54) / 3.0f;
        var actualSpan = (DateTime.Now - _initialTime).TotalMilliseconds + _timeBias;
        if (Math.Abs(Math.Floor(actualSpan / WanderingBubbleInterval) -
                     Math.Floor((actualSpan + 8000) / WanderingBubbleInterval)) < 0.1)
            _timeBias += 8000;
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
        var actualSpan = timeSpan.TotalMilliseconds + _timeBias;
        if (Math.Floor(actualSpan / WanderingBubbleInterval) > _wanderingBubbleRound)
        {
            _wanderingBubbleRound = Convert.ToInt32(actualSpan / WanderingBubbleInterval);
            _currRandX = _random.Next(-54, 54) / 3.0f;
        }
        var remainder = float.Parse(
            (actualSpan - _wanderingBubbleRound * WanderingBubbleInterval).ToString(CultureInfo.InvariantCulture));
        wanderingBubble.localPosition = new Vector3(_currRandX, remainder * 0.002f - 11f, 6);
    }
}