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

    private readonly int[] _bannedIndices = {1, 4, 5, 8, 10, 13};

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

    private readonly Timer[] _timers = new Timer[Constants.BanNum];

    public Button doneButton;

    private void Awake()
    {
        var replay = PlayerPrefs.GetString("replay");
        if (replay.Length > 0)
        {
            var fishAvailable = JsonMapper.ToObject(replay)[1]["players"][0]["my_fish"];
            Debug.Log(fishAvailable.Count);
        }
        for (var i = 0; i < Constants.FishNum; i++)
        {
            _entranceSpeedX.Add(_random.Next(-4, 5));
            _entranceSpeedY.Add(_random.Next(-4, 5));
        }
        for (var i = 0; i < 3; i++)
        {
            for (var j = 0; j < 6; j++)
            {
                var id = i * 6 + j;
                _fishTransforms[id] = Instantiate(PrefabRefs.FishPrefabs[id], allFishRoot);
                _targetPositions[id] = new Vector3(j * 3 - 7.5f, -i * 3);
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
            if (_bannedIndices.Contains(id)) continue;
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
        SceneManager.LoadScene("Scenes/Game");
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
            if (timeDiff > EntranceDuration)
            {
                _animationPlayed = true;
                for (var i = 0; i < Constants.BanNum; i++)
                {
                    var id = i;
                    _timers[id] = new Timer(
                        state =>
                        {
                            _uiQueue.Enqueue(() =>
                            {
                                var banBubble = Instantiate(bubblePrefab, allFishRoot);
                                banBubble.localPosition = _targetPositions[_bannedIndices[id]];
                                banBubble.localScale = new Vector3(2, 2, 2);
                                if (id != 5) return;
                                foreach (var timer in _timers) timer.Dispose();
                                ActivateFishTriggers();
                            });
                        },
                        null, i * 500 + 800, 0);
                }
            }
        }
        else
        {
            doneButton.interactable = _fishSelected.Count(b => b) == 4;
            while (_uiQueue.Count > 0)
                _uiQueue.Dequeue()();
        }
    }
}