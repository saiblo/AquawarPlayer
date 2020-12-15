using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using LitJson;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;

public class Preparation : MonoBehaviour
{
    public Transform allFishRoot;

    public Transform bubblePrefab;

    private readonly Transform[] _fishTransforms = new Transform[Constants.FishNum];

    private readonly Vector3[] _targetPositions = new Vector3[Constants.FishNum];

    private readonly EventTrigger[] _fishEventTriggers = new EventTrigger[Constants.FishNum];

    private readonly List<int> _entranceSpeedX = new List<int>();

    private readonly List<int> _entranceSpeedY = new List<int>();

    private readonly Random _random = new Random(Convert.ToInt32(DateTime.Now.Ticks % 100000000));

    private readonly List<int> _availableFish = new List<int>();

    private DateTime _initialTime;

    private bool _animationPlayed;

    private const float EntranceDuration = 1200.0f;

    private readonly bool[] _fishSelected =
    {
        false, false, false, false, false, false,
        false, false, false, false, false, false,
        false, false, false, false, false, false
    };

    private readonly Queue<Action> _uiQueue = new Queue<Action>();

    public Button doneButton;

    private Constants.GameMode _mode;

    private void Awake()
    {
        var replayStr = PlayerPrefs.GetString("replay");
        if (replayStr.Length > 0)
        {
            var replay = JsonMapper.ToObject(replayStr);
            Debug.Log(replay.Count);
            _mode = Constants.GameMode.Offline;
        }
        else
        {
            _mode = Constants.GameMode.Online;
        }
        for (var i = 0; i < Constants.FishNum; i++)
        {
            _entranceSpeedX.Add(_random.Next(-4, 5));
            _entranceSpeedY.Add(_random.Next(-4, 5));
        }
        for (var i = 0; i < 3; i++)
        {
            for (var j = 0; j < 4; j++)
            {
                var id = i * 4 + j;
                _fishTransforms[id] = Instantiate(PrefabRefs.FishPrefabs[id], allFishRoot);
                _fishTransforms[id].rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                _targetPositions[id] = new Vector3(j * 4 - 6, -i * 3);
                _fishEventTriggers[id] = _fishTransforms[id].GetComponent<EventTrigger>();
            }
        }
        _initialTime = DateTime.Now;
    }

    private void ActivateFishTriggers()
    {
        for (var i = 0; i < Constants.FishNum; i++)
        {
            var id = i;
            if (!_availableFish.Contains(id)) continue;
            var trigger = new EventTrigger.Entry();
            trigger.callback.AddListener(delegate
            {
                _fishSelected[id] = !_fishSelected[id];
                _fishTransforms[id].localScale = _fishSelected[id] ? new Vector3(4, 4, 4) : new Vector3(3, 3, 3);
            });
            _fishEventTriggers[id].triggers.Add(trigger);
        }
    }

    public void ConfirmSelection()
    {
        if (_mode == Constants.GameMode.Online)
        {
            var chooseFishs = new List<int>();
            for (var i = 0; i < Constants.FishNum; i++)
                if (_fishSelected[i])
                    chooseFishs.Add(i);
            Client.GameClient.Send(new Pick {ChooseFishs = chooseFishs});
        }
        SceneManager.LoadScene("Scenes/Game");
    }

    private void OfflineSelect()
    {
        _fishTransforms[2].localScale = new Vector3(4, 4, 4);
        _fishTransforms[6].localScale = new Vector3(4, 4, 4);
        _fishTransforms[7].localScale = new Vector3(4, 4, 4);
        _fishTransforms[9].localScale = new Vector3(4, 4, 4);
        doneButton.interactable = true;
    }

    private void Update()
    {
        if (!_animationPlayed)
        {
            var timeDiff =
                float.Parse((DateTime.Now - _initialTime).TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
            for (var i = 0; i < Constants.FishNum; i++)
            {
                _fishTransforms[i].localPosition =
                    _targetPositions[i] - new Vector3(
                        Math.Max(0, EntranceDuration - timeDiff) / 200f * (_entranceSpeedX[i] - 0.5f),
                        Math.Max(0, EntranceDuration - timeDiff) / 200f * (_entranceSpeedY[i] - 0.5f)
                    );
            }
            // ReSharper disable once InvertIf
            if (timeDiff > EntranceDuration && !_animationPlayed)
            {
                _animationPlayed = true;
                if (PlayerPrefs.GetInt("cursor", 0) == 0)
                {
                    if (_mode == Constants.GameMode.Offline)
                    {
                        _availableFish.Clear();
                        for (var i = 0; i < 12; i++) _availableFish.Add(i);
                        PlayerPrefs.SetInt("cursor", 3);
                    }
                    else
                    {
                        /*var result = await Client.GameClient.Receive();
                        if ((string) result["Action"] == "Pick")
                        {
                            var remaining = result["RemainFishs"];
                            _availableFish.Clear();
                            for (var i = 0; i < remaining.Count; i++)
                                _availableFish.Add((int) remaining[i]);
                        }*/
                        _availableFish.Clear();
                        for (var i = 0; i < 12; i++) _availableFish.Add(i);
                    }
                    Timer timer = null;
                    timer = new Timer(state =>
                        {
                            _uiQueue.Enqueue(() =>
                            {
                                for (var i = 0; i < Constants.FishNum; i++)
                                {
                                    if (_availableFish.Contains(i)) continue;
                                    var banBubble = Instantiate(bubblePrefab, allFishRoot);
                                    banBubble.localPosition = _targetPositions[i];
                                    banBubble.localScale = new Vector3(2, 2, 2);
                                }
                                // ReSharper disable once AccessToModifiedClosure
                                timer?.Dispose();
                                if (_mode == Constants.GameMode.Offline)
                                    OfflineSelect();
                                else
                                    ActivateFishTriggers();
                            });
                        },
                        null, 800, 0);
                }
                else if (_mode == Constants.GameMode.Offline)
                {
                    PlayerPrefs.SetInt("cursor", PlayerPrefs.GetInt("cursor") + 1);
                    OfflineSelect();
                }
            }
        }
        else
        {
            if (_mode == Constants.GameMode.Online)
            {
                doneButton.interactable = _fishSelected.Count(b => b) == 4;
            }
            while (_uiQueue.Count > 0)
                _uiQueue.Dequeue()();
        }
    }
}