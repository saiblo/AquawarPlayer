using System;
using System.Collections.Generic;
using System.Linq;
using GameHelper;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;
using Random = System.Random;

public class Preparation : EnhancedMonoBehaviour
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

    private bool _animationPlayed;

    private readonly bool[] _fishSelected =
    {
        false, false, false, false, false, false,
        false, false, false, false, false, false
    };

    public InputField imitate;

    public Button doneButton;

    private void Awake()
    {
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
                _fishTransforms[id] = Instantiate(
                    SharedRefs.FishPrefabs[id],
                    new Vector3(),
                    Quaternion.Euler(new Vector3(0, 180, 0)),
                    allFishRoot
                );
                _targetPositions[id] = new Vector3(j * 4 - 6, -i * 3);
                _fishEventTriggers[id] = _fishTransforms[id].GetComponent<EventTrigger>();
            }
        }

        Repeat(cnt =>
            {
                for (var i = 0; i < Constants.FishNum; i++)
                    _fishTransforms[i].localPosition =
                        _targetPositions[i] - new Vector3(
                            (199 - cnt) / 33f * (_entranceSpeedX[i] - 0.5f),
                            (199 - cnt) / 33f * (_entranceSpeedY[i] - 0.5f)
                        );
            },
            async () =>
            {
                _animationPlayed = true;
                if (SharedRefs.Mode == Constants.GameMode.Offline)
                {
                    _availableFish.Clear();
                    var remainingFish =
                        SharedRefs.ReplayJson[SharedRefs.ReplayCursor == 0 ? 1 : SharedRefs.ReplayCursor - 1]["players"]
                            [0]["my_fish"];
                    for (var i = 0; i < remainingFish.Count; i++)
                        _availableFish.Add((int) remainingFish[i]["id"] - 1);
                    if (SharedRefs.ReplayCursor == 0) SharedRefs.ReplayCursor = 1;
                }
                else
                {
                    var result = await SharedRefs.GameClient.Receive();
                    if ((string) result["Action"] == "Pick")
                    {
                        var remaining = result["RemainFishs"];
                        _availableFish.Clear();
                        for (var i = 0; i < remaining.Count; i++)
                            _availableFish.Add((int) remaining[i] - 1);
                    }
                }
                for (var i = 0; i < Constants.FishNum; i++)
                {
                    if (_availableFish.Contains(i)) continue;
                    var banBubble = Instantiate(bubblePrefab, allFishRoot);
                    banBubble.localPosition = _targetPositions[i];
                    banBubble.localScale = new Vector3(3, 3, 3);
                }
                if (SharedRefs.Mode == Constants.GameMode.Offline)
                    OfflineSelect();
                else
                    ActivateFishTriggers();
            }, 200, 0, 6);
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
        if (SharedRefs.Mode == Constants.GameMode.Online)
        {
            var chooseFishs = new List<int>();
            for (var i = 0; i < Constants.FishNum; i++)
                if (_fishSelected[i])
                    chooseFishs.Add(i + 1);
            SharedRefs.GameClient.Send(
                _fishSelected[11]
                    ? new Pick {ChooseFishs = chooseFishs, ImitateFish = Convert.ToInt32(imitate.text)}
                    : new Pick {ChooseFishs = chooseFishs}
            );
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

    protected override void RunPerFrame()
    {
        doneButton.interactable = _animationPlayed &&
                                  (SharedRefs.Mode == Constants.GameMode.Offline ||
                                   _fishSelected.Count(b => b) == 4 &&
                                   (!_fishSelected[11] || Convert.ToInt32(imitate.text) < 12));
    }
}