using System;
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
    private readonly int[] _myFishId = {0, 0, 0, 0};
    private readonly int[] _enemyFishId = {0, 0, 0, 0};

    private bool _myTurn = true;
    private bool _normalAttack = true;

    public Transform statusBarPrefab;

    public Transform allFishRoot;
    public Transform myStatusRoot;
    public Transform enemyStatusRoot;

    public InputField assertion;
    public Image normalButton;
    public Image skillButton;

    public Button changeStatusButton;
    public Text changeStatusPrompt;

    private readonly Vector3 _small = new Vector3(3, 3, 3);
    private readonly Vector3 _large = new Vector3(4, 4, 4);

    private readonly Queue<Action> _uiQueue = new Queue<Action>();

    private Constants.GameMode _mode;

    private JsonData _replay;

    public Transform questionPrefab;

    private readonly List<Transform> _myQuestions = new List<Transform>();

    private readonly List<Transform> _enemyQuestions = new List<Transform>();

    public Transform waterProjectile;

    public Material dissolveEffect;
    private int _dissolveShaderProperty;
    public AnimationCurve fadeIn;

    private void Dissolve(Renderer meshRenderer, ParticleSystem ps)
    {
        meshRenderer.material = dissolveEffect;
        ps.Play();
        Timer timer = null;
        var internalTick = 0;
        timer = new Timer(state =>
        {
            if (internalTick <= 300)
            {
                _uiQueue.Enqueue(() =>
                {
                    meshRenderer.material.SetFloat(_dissolveShaderProperty,
                        // ReSharper disable once AccessToModifiedClosure
                        fadeIn.Evaluate(Mathf.InverseLerp(0, 3, internalTick / 100f)));
                });
            }
            else
            {
                // ReSharper disable once AccessToModifiedClosure
                timer?.Dispose();
                // Destroy(meshRenderer.gameObject);
            }
            internalTick++;
        }, null, 0, 10);
    }

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

    private readonly List<Renderer> _myFishRenderers = new List<Renderer>();
    private readonly List<Renderer> _enemyFishRenderers = new List<Renderer>();

    private readonly List<ParticleSystem> _myFishParticleSystems = new List<ParticleSystem>();
    private readonly List<ParticleSystem> _enemyFishParticleSystems = new List<ParticleSystem>();

    private readonly int[] _myFishFullHp = {0, 0, 0, 0};
    private readonly int[] _enemyFishFullHp = {0, 0, 0, 0};

    public void SwitchToNormal()
    {
        _normalAttack = true;
    }

    public void SwitchToSkill()
    {
        _normalAttack = false;
    }

    private readonly List<int> _passiveList = new List<int>();

    private int _myFishSelected = -1;

    private int _enemyFishSelected = -1;

    private int _assertion = -1;
    private int _assertionPlayer;
    private int _assertionTarget; // Which fish do you think it is?

    private readonly bool[] _myFishSelectedAsTarget = {false, false, false, false};
    private readonly bool[] _enemyFishSelectedAsTarget = {false, false, false, false};

    private readonly List<Slider> _myStatus = new List<Slider>();
    private readonly List<Slider> _enemyStatus = new List<Slider>();

    public Transform explodePrefab;
    public Transform bigExplosion;
    public Transform recoverEffect;
    public Transform shieldEffect;

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
                    _assertionTarget = (int) operation["id"] - 1;
                    var guessFish = Instantiate(PrefabRefs.FishPrefabs[_assertionTarget], allFishRoot);
                    guessFish.localPosition =
                        FishRelativePosition(_assertionPlayer == 0, _assertion) + new Vector3(0, 6, 0);
                    SetTimeout(() =>
                    {
                        Destroy(guessFish.gameObject);
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
                        if (operation.ContainsKey("EnemyPos"))
                        {
                            _enemyFishSelectedAsTarget[(int) operation["EnemyPos"]] = true;
                            _normalAttack = true;
                        }
                        else
                        {
                            var enemyList = operation["EnemyList"];
                            for (var i = 0; i < enemyList.Count; i++)
                            {
                                _enemyFishSelectedAsTarget[(int) enemyList[i]] = true;
                            }
                            _normalAttack = false;
                        }
                        ChangeStatus();
                    }
                    else
                    {
                        _enemyFishSelected = (int) operation["MyPos"];
                        ChangeStatus();
                        if (operation.ContainsKey("EnemyPos"))
                        {
                            _myFishSelectedAsTarget[(int) operation["EnemyPos"]] = true;
                            _normalAttack = true;
                        }
                        else
                        {
                            var enemyList = operation["EnemyList"];
                            for (var i = 0; i < enemyList.Count; i++)
                            {
                                _myFishSelectedAsTarget[(int) enemyList[i]] = true;
                            }
                            _normalAttack = false;
                        }
                        ChangeStatus();
                    }
                }
                if (_replay[PlayerPrefs.GetInt("cursor")] != null)
                {
                    var players = _replay[PlayerPrefs.GetInt("cursor")]["players"];
                    for (var i = 0; i < 4; i++)
                    {
                        if ((float) players[0]["fight_fish"][i]["hp"] <= 0 &&
                            (float) _replay[PlayerPrefs.GetInt("cursor") - 2]["players"][0]["fight_fish"][i]["hp"] > 0)
                            Dissolve(_myFishRenderers[i], _myFishParticleSystems[i]);
                        if ((float) players[1]["fight_fish"][i]["hp"] <= 0 &&
                            (float) _replay[PlayerPrefs.GetInt("cursor") - 2]["players"][1]["fight_fish"][i]["hp"] > 0)
                            Dissolve(_enemyFishRenderers[i], _enemyFishParticleSystems[i]);
                    }
                    DisplayHp(players);
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

    private async void ChangeStatus()
    {
        switch (_selectStatus)
        {
            case SelectStatus.DoAssertion:
            {
                _selectStatus = SelectStatus.WaitAssertion;
                if (_assertion != -1)
                {
                    var hit = _assertionTarget == (_assertionPlayer == 1 ? _myFishId : _enemyFishId)[_assertion];
                    if (hit)
                    {
                        Destroy((_assertionPlayer == 1 ? _myQuestions : _enemyQuestions)[_assertion].gameObject);
                    }
                    for (var i = 0; i < 4; i++)
                    {
                        Instantiate(explodePrefab, allFishRoot).localPosition =
                            FishRelativePosition((_assertionPlayer == 1) ^ hit, i);
                    }
                    SetTimeout(() =>
                    {
                        _assertion = -1;
                        ChangeStatus();
                        if (_mode == Constants.GameMode.Offline) ProcessOffline();
                    }, 1000);
                }
                else
                {
                    ChangeStatus();
                }
                if (_myTurn)
                {
                    if (_assertion == -1)
                        await Client.GameClient.Send(new Null());
                    else
                        await Client.GameClient.Send(
                            new Assert {Pos = _assertion, Id = Convert.ToInt32(assertion.text)}
                        );
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
                _passiveList.Clear();
                var enemy = _enemyFishSelected >= 0 && _enemyFishSelected < 4;
                if (_normalAttack)
                {
                    var selected = enemy ? _enemyFishSelected : _myFishSelected;
                    var target = 0;
                    for (var i = 0; i < 4; i++)
                    {
                        // ReSharper disable once InvertIf
                        if (enemy && _myFishSelectedAsTarget[i] || _enemyFishSelectedAsTarget[i])
                        {
                            target = i;
                            break;
                        }
                    }
                    var distance = FishRelativePosition(enemy, selected) - FishRelativePosition(!enemy, target);
                    for (var i = 0; i <= 80; i++)
                    {
                        var id = i;
                        SetTimeout(() =>
                        {
                            (enemy ? _enemyFishTransforms : _myFishTransforms)[selected].localPosition =
                                FishRelativePosition(!enemy, target) + Math.Abs(id - 40f) / 40f * distance;
                        }, i * 10);
                    }
                    _passiveList.Add(target);
                }
                else
                {
                    var attackerTransforms = enemy ? _enemyFishTransforms : _myFishTransforms;
                    var attackeeTransforms = enemy ? _myFishTransforms : _enemyFishTransforms;
                    var attackerSelected = enemy ? _enemyFishSelectedAsTarget : _myFishSelectedAsTarget;
                    var attackeeSelected = enemy ? _myFishSelectedAsTarget : _enemyFishSelectedAsTarget;
                    var attacker = enemy ? _enemyFishSelected : _myFishSelected;
                    switch (enemy ? _enemyFishId[_enemyFishSelected] : _myFishId[_myFishSelected])
                    {
                        case 0:
                        case 2:
                        case 10:
                            for (int cnt = 0, i = 0; i < 4; i++)
                            {
                                if (!attackeeSelected[i]) continue;
                                var id = i;
                                SetTimeout(() =>
                                {
                                    var originalDistance = FishRelativePosition(!enemy, id) -
                                                           FishRelativePosition(enemy, attacker);
                                    var distance = originalDistance.x < 0
                                        ? originalDistance + new Vector3(4.5f, 0, 0)
                                        : originalDistance - new Vector3(4.5f, 0, 0);
                                    Instantiate(
                                        waterProjectile,
                                        FishRelativePosition(enemy, attacker) + new Vector3(3, 0, 0) *
                                        (enemy ? -1 : 1),
                                        Quaternion.Euler(
                                            new Vector3(0,
                                                Convert.ToInt32(Math.Atan(distance.x / distance.z) / Math.PI * 180.0),
                                                0)
                                        )
                                    );
                                }, cnt * 120);
                                _passiveList.Add(i);
                                cnt++;
                            }
                            break;
                        case 1:
                        case 3:
                            var poorFish = 0;
                            for (var i = 0; i < 4; i++)
                            {
                                // ReSharper disable once InvertIf
                                if (attackerSelected[i])
                                {
                                    poorFish = i;
                                    break;
                                }
                            }
                            var myFishExplode = Instantiate(bigExplosion, allFishRoot);
                            myFishExplode.localPosition = FishRelativePosition(enemy, poorFish);
                            SetTimeout(() => { Destroy(myFishExplode.gameObject); }, 2000);
                            var myFishRecover = Instantiate(recoverEffect, allFishRoot);
                            myFishRecover.localPosition = FishRelativePosition(enemy, attacker);
                            SetTimeout(() => { Destroy(myFishRecover.gameObject); }, 4000);
                            break;
                        case 4:
                        case 6:
                        case 8:
                        case 9:
                            for (var i = 0; i < 4; i++)
                            {
                                // ReSharper disable once InvertIf
                                if (enemy && _myFishSelectedAsTarget[i] || _enemyFishSelectedAsTarget[i])
                                {
                                    var target = i;
                                    var distance = FishRelativePosition(enemy, attacker) -
                                                   FishRelativePosition(!enemy, target);
                                    for (var j = 0; j <= 40; j++)
                                    {
                                        var id = j;
                                        SetTimeout(() =>
                                        {
                                            (enemy ? _enemyFishTransforms : _myFishTransforms)[attacker].localPosition =
                                                FishRelativePosition(!enemy, target) +
                                                Math.Abs(id - 20f) / 20f * distance;
                                        }, j * 10);
                                    }
                                    SetTimeout(() =>
                                    {
                                        var targetExplode = Instantiate(explodePrefab, allFishRoot);
                                        targetExplode.localPosition = FishRelativePosition(!enemy, target);
                                        SetTimeout(() => { Destroy(targetExplode.gameObject); }, 1000);
                                    }, 200);
                                    _passiveList.Add(i);
                                    break;
                                }
                            }
                            break;
                        case 5:
                        case 7:
                            var friendId = attacker;
                            for (var i = 0; i < 4; i++)
                            {
                                // ReSharper disable once InvertIf
                                if (attackeeSelected[i])
                                {
                                    friendId = i;
                                    break;
                                }
                            }
                            var shield = Instantiate(shieldEffect, allFishRoot);
                            shield.localPosition = FishRelativePosition(enemy, friendId);
                            SetTimeout(() => { Destroy(shield.gameObject); }, 5000);

                            var myselfRecover = Instantiate(recoverEffect, allFishRoot);
                            myselfRecover.localPosition = FishRelativePosition(enemy, attacker);
                            SetTimeout(() => { Destroy(myselfRecover.gameObject); }, 4000);
                            break;
                    }
                }
                var shouldProcessPassiveAttack = _passiveList.Count > 0;
                SetTimeout(() =>
                {
                    _myFishSelected = -1;
                    _enemyFishSelected = -1;
                    _uiQueue.Enqueue(() =>
                    {
                        for (var i = 0; i < 4; i++)
                        {
                            _myFishSelectedAsTarget[i] = _enemyFishSelectedAsTarget[i] = false;
                            _myFishTransforms[i].rotation = Quaternion.Euler(new Vector3(0, 100, 0));
                            _enemyFishTransforms[i].rotation = Quaternion.Euler(new Vector3(0, 260, 0));
                        }
                    });
                    _selectStatus = SelectStatus.DoAssertion;
                }, 1000);
                // }, shouldProcessPassiveAttack ? 2000 : 1000);
                /* _passiveList.ForEach((id) =>
                {
                    switch (id)
                    {
                        case 0:
                        case 1:
                            break;
                    }
                }); */
                break;
            }
            case SelectStatus.WaitingAnimation:
                if (_mode == Constants.GameMode.Online && _myTurn)
                {
                    await Client.GameClient.Receive(); // ACTION
                }
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
        }
        else
        {
            _mode = Constants.GameMode.Online;
        }
        var pickFish = _replay[PlayerPrefs.GetInt("cursor")]["operation"][0]["Fish"];
        var next = _replay[PlayerPrefs.GetInt("cursor") + 1];
        for (var i = 0; i < 4; i++)
        {
            _myFishId[i] = (int) pickFish[0][i]["id"] - 1;
            _enemyFishId[i] = (int) pickFish[1][i]["id"] - 1;
            if (_myFishId[i] > 11)
            {
                _myFishId[i] = 11;
            }
            if (_enemyFishId[i] > 11)
            {
                _enemyFishId[i] = 11;
            }
            _myFishFullHp[i] = (int) next["players"][0]["fight_fish"][i]["hp"];
            _enemyFishFullHp[i] = (int) next["players"][1]["fight_fish"][i]["hp"];
        }
        PlayerPrefs.SetInt("cursor", PlayerPrefs.GetInt("cursor") + 1);
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
            myFish.localPosition = FishRelativePosition(false, i);
            myFish.localScale = _small;
            myFish.rotation = Quaternion.Euler(new Vector3(0, 100, 0));
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
            _myFishRenderers.Add(myFish.GetComponentInChildren<Renderer>());
            _myFishParticleSystems.Add(myFish.GetComponentInChildren<ParticleSystem>());

            var myQuestion = Instantiate(questionPrefab, allFishRoot);
            myQuestion.localPosition = FishRelativePosition(false, i) + new Vector3(0, 4, 0);
            myQuestion.rotation =
                Quaternion.Euler(new Vector3(0, -Convert.ToInt32(Math.Atan(3.0 * (i + 1) / (17 - i))), 0));
            _myQuestions.Add(myQuestion);

            var enemyFish = Instantiate(PrefabRefs.FishPrefabs[_enemyFishId[i]], allFishRoot);
            enemyFish.localPosition = FishRelativePosition(true, i);
            enemyFish.localScale = _small;
            enemyFish.rotation = Quaternion.Euler(new Vector3(0, 260, 0));
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
            _enemyFishRenderers.Add(enemyFish.GetComponentInChildren<Renderer>());
            _enemyFishParticleSystems.Add(enemyFish.GetComponentInChildren<ParticleSystem>());

            var enemyQuestion = Instantiate(questionPrefab, allFishRoot);
            enemyQuestion.localPosition = FishRelativePosition(true, i) + new Vector3(0, 4, 0);
            enemyQuestion.rotation =
                Quaternion.Euler(new Vector3(0, Convert.ToInt32(Math.Atan(3.0 * (i + 1) / (17 - i))), 0));
            _enemyQuestions.Add(enemyQuestion);
        }

        _dissolveShaderProperty = Shader.PropertyToID("_cutoff");

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

        (_normalAttack ? normalButton : skillButton).color = Color.green;
        (_normalAttack ? skillButton : normalButton).color = Color.blue;
    }

    private static Vector3 FishRelativePosition(bool enemy, int id)
    {
        return new Vector3(
            (enemy ? 1 : -1) * 3 * (id + 1),
            0,
            2 - id
        );
    }
}