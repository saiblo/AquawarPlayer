﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LitJson;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
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

    private JsonData _replay;

    public Transform questionPrefab;

    private readonly List<Transform> _myQuestions = new List<Transform>();

    private readonly List<Transform> _enemyQuestions = new List<Transform>();

    private enum SelectStatus
    {
        DoAssertion,
        WaitAssertion,
        SelectMyFish,
        SelectEnemyFish,
        WaitingAnimation
    }

    private SelectStatus _selectStatus = SelectStatus.DoAssertion;

    private readonly List<Transform> _myFishTransforms = new List<Transform>();
    private readonly List<Transform> _enemyFishTransforms = new List<Transform>();

    private readonly int[] _myFishFullHp = {0, 0, 0, 0};
    private readonly int[] _enemyFishFullHp = {0, 0, 0, 0};

    private int _myFishSelected = -1;

    private int _enemyFishSelected = -1;

    private int _assertion = -1;
    private int _assertionPlayer;
    private int _assertionTarget; // Which fish do you think it is?

    private readonly bool[] _myFishSelectedAsTarget = {false, false, false, false};
    private readonly bool[] _enemyFishSelectedAsTarget = {false, false, false, false};

    private readonly List<Slider> _myStatus = new List<Slider>();
    private readonly List<Slider> _enemyStatus = new List<Slider>();

    private void SetTimeout(Action action, int timeout)
    {
        Timer timer = null;
        timer = new Timer(state =>
            {
                _uiQueue.Enqueue(action);
                // ReSharper disable once AccessToModifiedClosure
                timer?.Dispose();
            }
            , null, timeout, 0);
    }

    private void DisplayHp(JsonData players)
    {
        for (var i = 0; i < 4; i++)
        {
            _myStatus[i].value = (float) players[0]["fight_fish"][i]["hp"] / _myFishFullHp[i];
            _enemyStatus[i].value = (float) players[1]["fight_fish"][i]["hp"] / _enemyFishFullHp[i];
        }
    }

    private void ProcessOffline()
    {
        var state = _replay[PlayerPrefs.GetInt("cursor")];
        switch ((int) state["gamestate"])
        {
            case 2:
                SceneManager.LoadScene("Scenes/Preparation");
                break;
            case 3:
            {
                PlayerPrefs.SetInt("cursor", PlayerPrefs.GetInt("cursor") + 1);
                var operation = state["operation"][0];
                if ((string) operation["Action"] == "Assert")
                {
                    _assertionPlayer = (int) operation["ID"];
                    _assertion = (int) operation["Pos"];
                    _assertionTarget = (int) operation["id"];
                    var guessFish = Instantiate(PrefabRefs.FishPrefabs[_assertionTarget], allFishRoot);
                    guessFish.localPosition =
                        (_assertionPlayer == 1 ? _myFishTransforms : _enemyFishTransforms)[_assertion]
                        .localPosition + new Vector3(0, 6, 0);
                    SetTimeout(() =>
                    {
                        // Any better approach?
                        guessFish.localPosition = new Vector3(100, 100, 100);
                        ChangeStatus();
                    }, 2000);
                }
                else
                {
                    _assertion = -1;
                    ChangeStatus();
                    ChangeStatus();
                    SetTimeout(ProcessOffline, 400);
                }
                break;
            }
            case 4:
            {
                PlayerPrefs.SetInt("cursor", PlayerPrefs.GetInt("cursor") + 1);
                var operation = state["operation"][0];
                if ((string) operation["Action"] == "Action")
                {
                    if ((int) operation["ID"] == 0)
                    {
                        _myFishSelected = (int) operation["MyPos"];
                        ChangeStatus();
                        _enemyFishSelectedAsTarget[(int) operation["EnemyPos"]] = true;
                        ChangeStatus();
                    }
                    else
                    {
                        _enemyFishSelected = (int) operation["MyPos"];
                        ChangeStatus();
                        _myFishSelectedAsTarget[(int) operation["EnemyPos"]] = true;
                        ChangeStatus();
                    }
                }
                if (_replay[PlayerPrefs.GetInt("cursor")] != null)
                {
                    DisplayHp(_replay[PlayerPrefs.GetInt("cursor")]["players"]);
                    SetTimeout(ProcessOffline, 3000);
                }
                break;
            }
            default:
                PlayerPrefs.SetInt("cursor", PlayerPrefs.GetInt("cursor") + 1);
                SetTimeout(ProcessOffline, 100);
                break;
        }
    }

    private void ChangeStatus()
    {
        switch (_selectStatus)
        {
            case SelectStatus.DoAssertion:
            {
                _selectStatus = SelectStatus.WaitAssertion;
                if (_assertion != -1)
                {
                    _internalTick = 0;
                    Timer timer = null;
                    if (_assertionTarget == (_assertionPlayer == 1 ? _myFishId : _enemyFishId)[_assertion])
                    {
                        _uiQueue.Enqueue(() =>
                        {
                            // Any better approach?
                            (_assertionPlayer == 1 ? _myQuestions : _enemyQuestions)[_assertion]
                                .localPosition = new Vector3(100, 100, 100);
                        });
                        timer = new Timer(state =>
                        {
                            _internalTick++;
                            if (_internalTick < 150)
                            {
                                _uiQueue.Enqueue(() =>
                                {
                                    var size = 8f - (_internalTick - 75) * (_internalTick - 75) / 1875f;
                                    (_assertionPlayer == 1 ? _myFishTransforms : _enemyFishTransforms)[_assertion]
                                        .localScale = new Vector3(size, size, size);
                                });
                            }
                            else
                            {
                                // ReSharper disable once AccessToModifiedClosure
                                timer?.Dispose();
                                _uiQueue.Enqueue(() => { _assertion = -1; });
                                SetTimeout(() =>
                                {
                                    ChangeStatus();
                                    ProcessOffline();
                                }, 800);
                            }
                        }, null, 0, 10);
                    }
                    else
                    {
                        timer = new Timer(state =>
                        {
                            _internalTick++;
                            if (_internalTick < 200)
                            {
                                _uiQueue.Enqueue(() =>
                                    {
                                        var transforms =
                                            _assertionPlayer == 0 ? _myFishTransforms : _enemyFishTransforms;
                                        for (var i = 0; i < 4; i++)
                                        {
                                            transforms[i].RotateAround(
                                                transforms[i].localPosition,
                                                Vector3.up,
                                                10f
                                            );
                                        }
                                    }
                                );
                            }
                            else
                            {
                                // ReSharper disable once AccessToModifiedClosure
                                timer?.Dispose();
                                _uiQueue.Enqueue(() =>
                                {
                                    for (var i = 0; i < 4; i++)
                                    {
                                        _myFishTransforms[i].rotation = Quaternion.Euler(new Vector3(0, 150, 0));
                                        _enemyFishTransforms[i].rotation = Quaternion.Euler(new Vector3(0, 210, 0));
                                    }
                                    _assertion = -1;
                                    ChangeStatus();
                                    ProcessOffline();
                                });
                            }
                        }, null, 0, 5);
                    }
                }
                break;
            }
            case SelectStatus.WaitAssertion:
                _selectStatus = SelectStatus.SelectMyFish;
                break;
            case SelectStatus.SelectMyFish:
                _selectStatus = SelectStatus.SelectEnemyFish;
                break;
            case SelectStatus.SelectEnemyFish:
            {
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
                            if (_myFishSelected >= 0 && _myFishSelected < 4)
                            {
                                _myFishTransforms[_myFishSelected].localPosition = new Vector3(
                                    -3 * (_myFishSelected + 1),
                                    1 - (_internalTick - 50) * (_internalTick - 50) / 2500f,
                                    2 - _myFishSelected
                                );
                            }
                            else if (_enemyFishSelected >= 0 && _enemyFishSelected < 4)
                            {
                                _enemyFishTransforms[_enemyFishSelected].localPosition = new Vector3(
                                    3 * (_enemyFishSelected + 1),
                                    1 - (_internalTick - 50) * (_internalTick - 50) / 2500f,
                                    2 - _enemyFishSelected
                                );
                            }
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
                        _enemyFishSelected = -1;
                        _uiQueue.Enqueue(() =>
                        {
                            for (var i = 0; i < 4; i++)
                            {
                                _myFishSelectedAsTarget[i] = _enemyFishSelectedAsTarget[i] = false;
                                _myFishTransforms[i].rotation = Quaternion.Euler(new Vector3(0, 150, 0));
                                _enemyFishTransforms[i].rotation = Quaternion.Euler(new Vector3(0, 210, 0));
                            }
                        });
                        _selectStatus = SelectStatus.DoAssertion;
                    }
                }, null, 0, 5);
                break;
            }
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
            _replay = JsonMapper.ToObject(replayStr);
            _mode = Constants.GameMode.Offline;
            var curr = _replay[PlayerPrefs.GetInt("cursor")];
            for (var i = 0; i < 4; i++)
            {
                _myFishFullHp[i] = (int) curr["players"][0]["fight_fish"][i]["hp"];
                _enemyFishFullHp[i] = (int) curr["players"][1]["fight_fish"][i]["hp"];
            }
        }
        else
        {
            _mode = Constants.GameMode.Online;
        }
        for (var i = 0; i < 4; i++)
        {
            var j = i;
            var myStatus = Instantiate(statusBarPrefab, myStatusRoot);
            myStatus.localPosition = new Vector3(10, -50 * i - 10);
            _myStatus.Add(myStatus.GetComponent<Slider>());
            var enemyStatus = Instantiate(statusBarPrefab, enemyStatusRoot);
            enemyStatus.localPosition = new Vector3(10, -50 * i - 10);
            _enemyStatus.Add(enemyStatus.GetComponent<Slider>());

            var myFish = Instantiate(PrefabRefs.FishPrefabs[_myFishId[i]], allFishRoot);
            myFish.localPosition = new Vector3(-3 * (i + 1), 0, 2 - i);
            myFish.localScale = _small;
            myFish.rotation = Quaternion.Euler(new Vector3(0, 150, 0));
            if (_mode == Constants.GameMode.Online)
            {
                var myFishTrigger = new EventTrigger.Entry();
                myFishTrigger.callback.AddListener(delegate
                {
                    switch (_selectStatus)
                    {
                        case SelectStatus.DoAssertion:
                            break;
                        case SelectStatus.WaitAssertion:
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
            }
            _myFishTransforms.Add(myFish);

            var myQuestion = Instantiate(questionPrefab, allFishRoot);
            myQuestion.localPosition = new Vector3(-3 * (i + 1), 4, 2 - i);
            myQuestion.rotation =
                Quaternion.Euler(new Vector3(0, -Convert.ToInt32(Math.Atan(3.0 * (i + 1) / (17 - i))), 0));
            _myQuestions.Add(myQuestion);

            var enemyFish = Instantiate(PrefabRefs.FishPrefabs[_enemyFishId[i]], allFishRoot);
            enemyFish.localPosition = new Vector3(3 * (i + 1), 0, 2 - i);
            enemyFish.localScale = _small;
            enemyFish.rotation = Quaternion.Euler(new Vector3(0, 210, 0));
            if (_mode == Constants.GameMode.Online)
            {
                var enemyFishTrigger = new EventTrigger.Entry();
                enemyFishTrigger.callback.AddListener(delegate
                {
                    switch (_selectStatus)
                    {
                        case SelectStatus.DoAssertion:
                            _assertion = _assertion == j ? -1 : j;
                            break;
                        case SelectStatus.WaitAssertion:
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
            }
            _enemyFishTransforms.Add(enemyFish);

            var enemyQuestion = Instantiate(questionPrefab, allFishRoot);
            enemyQuestion.localPosition = new Vector3(3 * (i + 1), 4, 2 - i);
            enemyQuestion.rotation =
                Quaternion.Euler(new Vector3(0, Convert.ToInt32(Math.Atan(3.0 * (i + 1) / (17 - i))), 0));
            _enemyQuestions.Add(enemyQuestion);
        }

        if (_mode == Constants.GameMode.Offline) ProcessOffline();
    }

    private void Update()
    {
        while (_uiQueue.Count > 0)
            _uiQueue.Dequeue()();

        if (_selectStatus != SelectStatus.WaitAssertion)
        {
            for (var i = 0; i < 4; i++)
            {
                if (_myFishSelectedAsTarget[i])
                    _myFishTransforms[i].localScale = _large;
                else if (_myFishSelected == i)
                    _myFishTransforms[i].localScale = _large;
                else
                    _myFishTransforms[i].localScale = _small;

                if (_enemyFishSelectedAsTarget[i])
                    _enemyFishTransforms[i].localScale = _large;
                else if (_enemyFishSelected == i)
                    _enemyFishTransforms[i].localScale = _large;
                else
                    _enemyFishTransforms[i].localScale = _small;
            }
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
            case SelectStatus.WaitAssertion:
                title = "请等待动画放完";
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