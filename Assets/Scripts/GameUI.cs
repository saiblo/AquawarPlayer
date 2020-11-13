using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LitJson;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    private readonly int[] _myFishId = {1, 3, 4, 5};
    private readonly int[] _enemyFishId = {3, 6, 7, 8};

    public Transform statusBarPrefab;

    public Transform allFishRoot;
    public Transform myStatusRoot;
    public Transform enemyStatusRoot;

    public Button changeStatusButton;
    public Text changeStatusPrompt;

    private readonly Vector3 _small = new Vector3(5, 5, 5);
    private readonly Vector3 _large = new Vector3(8, 8, 8);

    private readonly Queue<Action> _uiQueue = new Queue<Action>();

    private int _internalTick;

    private Constants.GameMode _mode;

    private enum SelectStatus
    {
        DoAssertion,
        SelectMyFish,
        SelectEnemyFish,
        WaitingAnimation
    }

    private SelectStatus _selectStatus = SelectStatus.DoAssertion;

    private readonly List<Transform> _myFishTransforms = new List<Transform>();
    private readonly List<Transform> _enemyFishTransforms = new List<Transform>();

    private int _myFishSelected = -1;

    private int _assertion = -1;

    private readonly bool[] _myFishSelectedAsTarget = {false, false, false, false};
    private readonly bool[] _enemyFishSelectedAsTarget = {false, false, false, false};

    public void ChangeStatus()
    {
        switch (_selectStatus)
        {
            case SelectStatus.DoAssertion:
                _selectStatus = SelectStatus.SelectMyFish;
                // if (_assertion != -1)
                // {
                _assertion = -1;
                // }
                break;
            case SelectStatus.SelectMyFish:
                _selectStatus = SelectStatus.SelectEnemyFish;
                break;
            case SelectStatus.SelectEnemyFish:
                _selectStatus = SelectStatus.WaitingAnimation;
                _internalTick = 0;
                Timer timer = null;
                timer = new Timer(state =>
                {
                    _internalTick++;
                    if (_internalTick < 100)
                    {
                        _uiQueue.Enqueue(() =>
                        {
                            if (_myFishSelected < 0 || _myFishSelected >= 4) return;
                            _myFishTransforms[_myFishSelected].localPosition = new Vector3(
                                -3 * (_myFishSelected + 2),
                                1 - (_internalTick - 50) * (_internalTick - 50) / 2500f,
                                2 - _myFishSelected
                            );
                        });
                    }
                    else if (_internalTick < 300)
                    {
                        _uiQueue.Enqueue(() =>
                        {
                            for (var i = 0; i < 4; i++)
                            {
                                if (_myFishSelectedAsTarget[i])
                                {
                                    _myFishTransforms[i].RotateAround(
                                        _myFishTransforms[i].localPosition,
                                        Vector3.up,
                                        10f
                                    );
                                }
                                if (_enemyFishSelectedAsTarget[i])
                                {
                                    _enemyFishTransforms[i].RotateAround(
                                        _enemyFishTransforms[i].localPosition,
                                        Vector3.up,
                                        10f
                                    );
                                }
                            }
                        });
                    }
                    else
                    {
                        // ReSharper disable once AccessToModifiedClosure
                        timer?.Dispose();
                        _myFishSelected = -1;
                        _uiQueue.Enqueue(() =>
                        {
                            for (var i = 0; i < 4; i++)
                            {
                                _myFishSelectedAsTarget[i] = _enemyFishSelectedAsTarget[i] = false;
                                _myFishTransforms[i].rotation = new Quaternion();
                                _enemyFishTransforms[i].rotation = new Quaternion();
                            }
                        });
                        _selectStatus = SelectStatus.SelectMyFish;
                    }
                }, null, 0, 5);
                break;
            case SelectStatus.WaitingAnimation:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

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
        for (var i = 0; i < 4; i++)
        {
            var j = i;
            Instantiate(statusBarPrefab, myStatusRoot)
                .localPosition = new Vector3(10, -15 * i - 10);
            Instantiate(statusBarPrefab, enemyStatusRoot)
                .localPosition = new Vector3(10, -15 * i - 10);

            var myFish = Instantiate(PrefabRefs.FishPrefabs[_myFishId[i]], allFishRoot);
            myFish.localPosition = new Vector3(-3 * (i + 2), 0, 2 - i);
            myFish.localScale = _small;
            var myFishTrigger = new EventTrigger.Entry();
            myFishTrigger.callback.AddListener(delegate
            {
                switch (_selectStatus)
                {
                    case SelectStatus.DoAssertion:
                        break;
                    case SelectStatus.SelectMyFish:
                        _myFishSelected = _myFishSelected == j ? -1 : j;
                        break;
                    case SelectStatus.SelectEnemyFish:
                        _myFishSelectedAsTarget[j] = !_myFishSelectedAsTarget[j];
                        break;
                    case SelectStatus.WaitingAnimation:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });
            myFish.GetComponent<EventTrigger>().triggers.Add(myFishTrigger);
            _myFishTransforms.Add(myFish);

            var enemyFish = Instantiate(PrefabRefs.FishPrefabs[_enemyFishId[i]], allFishRoot);
            enemyFish.localPosition = new Vector3(3 * (i + 2), 0, 2 - i);
            enemyFish.localScale = _small;
            var enemyFishTrigger = new EventTrigger.Entry();
            enemyFishTrigger.callback.AddListener(delegate
            {
                switch (_selectStatus)
                {
                    case SelectStatus.DoAssertion:
                        _assertion = _assertion == j ? -1 : j;
                        break;
                    case SelectStatus.SelectMyFish:
                        break;
                    case SelectStatus.SelectEnemyFish:
                        _enemyFishSelectedAsTarget[j] = !_enemyFishSelectedAsTarget[j];
                        break;
                    case SelectStatus.WaitingAnimation:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });
            enemyFish.GetComponent<EventTrigger>().triggers.Add(enemyFishTrigger);
            _enemyFishTransforms.Add(enemyFish);
        }
    }

    private void Update()
    {
        while (_uiQueue.Count > 0)
            _uiQueue.Dequeue()();

        for (var i = 0; i < 4; i++)
        {
            if (_myFishSelectedAsTarget[i])
                _myFishTransforms[i].localScale = _large;
            else if (_myFishSelected == i)
                _myFishTransforms[i].localScale = _large;
            else
                _myFishTransforms[i].localScale = _small;

            _enemyFishTransforms[i].localScale =
                _enemyFishSelectedAsTarget[i] || i == _assertion ? _large : _small;
        }

        changeStatusButton.interactable =
            _selectStatus == SelectStatus.DoAssertion ||
            _selectStatus == SelectStatus.SelectMyFish && _myFishSelected != -1 ||
            _selectStatus == SelectStatus.SelectEnemyFish &&
            (_myFishSelectedAsTarget.Any(b => b) ||
             _enemyFishSelectedAsTarget.Any(b => b));

        string title;
        switch (_selectStatus)
        {
            case SelectStatus.DoAssertion:
                title = _assertion == -1 ? "放弃断言" : "进行断言";
                break;
            case SelectStatus.SelectMyFish:
                title = "选择我方鱼";
                break;
            case SelectStatus.SelectEnemyFish:
                title = "选择作用对象";
                break;
            case SelectStatus.WaitingAnimation:
                title = "请等待动画放完";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        changeStatusPrompt.text = title;
    }
}