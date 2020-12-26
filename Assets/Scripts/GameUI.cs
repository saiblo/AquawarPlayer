using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LitJson;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : GameBridge
{
    private readonly GameStates _gameStates;

    private readonly GameObjectManager _gom;

    // Dissolve effect

    private int _dissolveShaderProperty;

    private void Dissolve(Renderer meshRenderer, ParticleSystem ps, Component fish, Component question)
    {
        meshRenderer.material = dissolveEffect;
        ps.Play();
        Repeat(cnt =>
            {
                meshRenderer.material.SetFloat(_dissolveShaderProperty,
                    fadeIn.Evaluate(Mathf.InverseLerp(0, 3, cnt / 100f)));
            },
            () =>
            {
                if (fish != null) Destroy(fish.gameObject);
                if (question != null) Destroy(question.gameObject);
            },
            300, 0, 10);
    }

    // UI related

    public void SwitchToNormal()
    {
        _gameStates.NormalAttack = true;
    }

    public void SwitchToSkill()
    {
        _gameStates.NormalAttack = false;
    }

    private void DisplayHp(JsonData players)
    {
        for (var i = 0; i < 4; i++)
        {
            _gom.MyStatus[i].value = (float) players[0]["fight_fish"][i]["hp"] / _gameStates.MyFishFullHp[i];
            _gom.EnemyStatus[i].value = (float) players[1]["fight_fish"][i]["hp"] / _gameStates.EnemyFishFullHp[i];
        }
    }

    /// <summary>
    ///   <para>First, resets the game states.<br/>
    /// After that, for online mode, listens to message from remote
    /// and determine what to show next; for offline mode, simply resets
    /// <code>_gameStatus</code> to <code>DoAssertion</code>.</para>
    /// </summary>
    private async void NewRound()
    {
        _gameStates.MyFishSelected = -1;
        _gameStates.EnemyFishSelected = -1;
        for (var i = 0; i < 4; i++)
            _gameStates.MyFishSelectedAsTarget[i] = _gameStates.EnemyFishSelectedAsTarget[i] = false;

        if (SharedRefs.Mode == Constants.GameMode.Offline)
        {
            _gameStates.GameStatus = Constants.GameStatus.DoAssertion;
        }
        else
        {
            var result = await Client.GameClient.Receive();
            _gameStates.GameStatus = Constants.GameStatus.DoAssertion;
            for (var i = 0; i < 4; i++)
            {
                _gameStates.MyFishId[i] = (int) result["MyFish"][i] - 1;
                if ((int) result["EnemyFish"][i] > 0)
                    _gameStates.EnemyFishId[i] = (int) result["EnemyFish"][i] - 1;
            }

            if (!_gom.Initialized) RunOnUiThread(() => { _gom.Init(unkFishPrefab, allFishRoot); });

            _gameStates.MyTurn = (string) result["Action"] == "Assert";
            if (_gameStates.MyTurn) return;

            _gameStates.AssertionPlayer = 1;
            if (result["AssertPos"] == null)
            {
                _gameStates.Assertion = -1;
                _gameStates.OnlineAssertionHit = false;
                _gameStates.AssertionTarget = 0;
                RunOnUiThread(() =>
                {
                    ChangeStatus(); // Skips the next two stages
                    ChangeStatus();
                });
                return;
            }

            _gameStates.Assertion = (int) result["AssertPos"];
            _gameStates.OnlineAssertionHit = (bool) result["AssertResult"];
            // _gameStates.AssertionTarget = (int) result["AssertContent"];
            RunOnUiThread(() =>
            {
                var guessFish = Instantiate(
                    SharedRefs.FishPrefabs[_gameStates.AssertionTarget],
                    GameObjectManager.FishRelativePosition(false, _gameStates.Assertion) + new Vector3(0, 6, 0),
                    Quaternion.Euler(new Vector3(0, 180, 0)),
                    allFishRoot);
                SetTimeout(() =>
                {
                    Destroy(guessFish.gameObject);
                    ChangeStatus(); // As if the enemy have decided which fish to assert
                }, 1200);
            });
            SetTimeout(ChangeStatus, 3000); // Just waits for the assertion animation to finish
        }
    }

    private void ProcessOffline()
    {
        var state = SharedRefs.ReplayJson[SharedRefs.ReplayCursor];
        switch ((int) state["gamestate"])
        {
            case 2:
                SceneManager.LoadScene("Scenes/Preparation");
                break;
            case 3:
            {
                SharedRefs.ReplayCursor++;
                var operation = state["operation"][0];
                if ((string) operation["Action"] == "Assert")
                {
                    _gameStates.AssertionPlayer = (int) operation["ID"];
                    _gameStates.Assertion = (int) operation["Pos"];
                    _gameStates.AssertionTarget = (int) operation["id"] - 1;
                    var guessFish = Instantiate(SharedRefs.FishPrefabs[_gameStates.AssertionTarget], allFishRoot);
                    guessFish.localPosition = GameObjectManager.FishRelativePosition(
                        _gameStates.AssertionPlayer == 0,
                        _gameStates.Assertion
                    ) + new Vector3(0, 6, 0);
                    SetTimeout(() =>
                    {
                        Destroy(guessFish.gameObject);
                        ChangeStatus();
                    }, 2000);
                }
                else
                {
                    _gameStates.Assertion = -1;
                    ChangeStatus();
                    ChangeStatus();
                    SetTimeout(ProcessOffline, 400);
                }
                break;
            }
            case 4:
            {
                SharedRefs.ReplayCursor++;
                var operation = state["operation"][0];
                if ((string) operation["Action"] == "Action")
                {
                    if ((int) operation["ID"] == 0)
                    {
                        _gameStates.MyFishSelected = (int) operation["MyPos"];
                        ChangeStatus();
                        if (operation.ContainsKey("EnemyPos"))
                        {
                            _gameStates.EnemyFishSelectedAsTarget[(int) operation["EnemyPos"]] = true;
                            _gameStates.NormalAttack = true;
                        }
                        else
                        {
                            var enemyList = operation["EnemyList"];
                            for (var i = 0; i < enemyList.Count; i++)
                            {
                                _gameStates.EnemyFishSelectedAsTarget[(int) enemyList[i]] = true;
                            }
                            _gameStates.NormalAttack = false;
                        }
                        ChangeStatus();
                    }
                    else
                    {
                        _gameStates.EnemyFishSelected = (int) operation["MyPos"];
                        ChangeStatus();
                        if (operation.ContainsKey("EnemyPos"))
                        {
                            _gameStates.MyFishSelectedAsTarget[(int) operation["EnemyPos"]] = true;
                            _gameStates.NormalAttack = true;
                        }
                        else
                        {
                            var enemyList = operation["EnemyList"];
                            for (var i = 0; i < enemyList.Count; i++)
                            {
                                _gameStates.MyFishSelectedAsTarget[(int) enemyList[i]] = true;
                            }
                            _gameStates.NormalAttack = false;
                        }
                        ChangeStatus();
                    }
                }
                if (SharedRefs.ReplayJson[SharedRefs.ReplayCursor] != null)
                {
                    var players = SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["players"];
                    var lastPlayers = SharedRefs.ReplayJson[SharedRefs.ReplayCursor - 2]["players"];
                    for (var i = 0; i < 4; i++)
                    {
                        if ((float) players[0]["fight_fish"][i]["hp"] <= 0 &&
                            (float) lastPlayers[0]["fight_fish"][i]["hp"] > 0)
                        {
                            _gameStates.MyFishAlive[i] = false;
                            Dissolve(
                                _gom.MyFishRenderers[i],
                                _gom.MyFishParticleSystems[i],
                                _gom.MyFishTransforms[i],
                                _gom.MyQuestions[i]
                            );
                        }
                        // ReSharper disable once InvertIf
                        if ((float) players[1]["fight_fish"][i]["hp"] <= 0 &&
                            (float) lastPlayers[1]["fight_fish"][i]["hp"] > 0)
                        {
                            _gameStates.EnemyFishAlive[i] = false;
                            Dissolve(
                                _gom.EnemyFishRenderers[i],
                                _gom.EnemyFishParticleSystems[i],
                                _gom.EnemyFishTransforms[i],
                                _gom.EnemyQuestions[i]
                            );
                        }
                    }
                    DisplayHp(players);
                    SetTimeout(ProcessOffline, 3000);
                }
                break;
            }
            default:
                SharedRefs.ReplayCursor++;
                SetTimeout(ProcessOffline, 100);
                break;
        }
    }

    private async void ChangeStatus()
    {
        switch (_gameStates.GameStatus)
        {
            case Constants.GameStatus.DoAssertion:
            {
                _gameStates.GameStatus = Constants.GameStatus.WaitAssertion;
                if (SharedRefs.Mode == Constants.GameMode.Online && _gameStates.MyTurn)
                {
                    if (_gameStates.Assertion == -1)
                        await Client.GameClient.Send(new Null());
                    else
                        await Client.GameClient.Send(
                            new Assert {Pos = _gameStates.Assertion, ID = Convert.ToInt32(assertion.text)}
                        );
                    var reply = await Client.GameClient.Receive(); // ASSERT_REPLY
                    _gameStates.AssertionPlayer = 0;
                    _gameStates.OnlineAssertionHit = (bool) (reply["AssertResult"] ?? false);
                }
                if (_gameStates.Assertion != -1)
                {
                    var hit =
                        SharedRefs.Mode == Constants.GameMode.Offline && _gameStates.AssertionTarget ==
                        (_gameStates.AssertionPlayer == 1 ? _gameStates.MyFishId : _gameStates.EnemyFishId)
                        [_gameStates.Assertion]
                        || SharedRefs.Mode == Constants.GameMode.Online && _gameStates.OnlineAssertionHit;
                    if (hit)
                    {
                        Destroy((_gameStates.AssertionPlayer == 1 ? _gom.MyQuestions : _gom.EnemyQuestions)
                            [_gameStates.Assertion].gameObject);
                        (_gameStates.AssertionPlayer == 1 ? _gameStates.MyFishExpose : _gameStates.EnemyFishExpose)
                            [_gameStates.Assertion] = true;
                        if (SharedRefs.Mode == Constants.GameMode.Online)
                        {
                            (_gameStates.AssertionPlayer == 1 ? _gameStates.MyFishId : _gameStates.EnemyFishId)
                                [_gameStates.Assertion] = _gameStates.AssertionTarget;
                            var transforms =
                                _gameStates.AssertionPlayer == 1 ? _gom.MyFishTransforms : _gom.EnemyFishTransforms;
                            Destroy(transforms[_gameStates.Assertion].gameObject);
                            transforms[_gameStates.Assertion] = _gom.GenFish(
                                _gameStates.AssertionPlayer == 0,
                                _gameStates.Assertion,
                                unkFishPrefab,
                                allFishRoot
                            );
                        }
                    }
                    for (var i = 0; i < 4; i++)
                        if (((_gameStates.AssertionPlayer == 1) ^ hit
                            ? _gameStates.EnemyFishAlive
                            : _gameStates.MyFishAlive)[i])
                            Instantiate(explodePrefab, allFishRoot).localPosition =
                                GameObjectManager.FishRelativePosition((_gameStates.AssertionPlayer == 1) ^ hit, i);
                    SetTimeout(async () =>
                    {
                        _gameStates.Assertion = -1;
                        ChangeStatus();
                        if (SharedRefs.Mode == Constants.GameMode.Offline)
                        {
                            ProcessOffline();
                        }
                        else if (!_gameStates.MyTurn)
                        {
                            await Client.GameClient.Receive(); // SUCCESS
                            await Client.GameClient.Send(new Ok());
                            _gameStates.EnemyFishSelected = 0;
                            _gameStates.NormalAttack = true;
                            _gameStates.MyFishSelectedAsTarget[0] = true;
                            ChangeStatus();
                            ChangeStatus();
                        }
                    }, 1000);
                }
                else
                {
                    RunOnUiThread(ChangeStatus);
                }
                if (SharedRefs.Mode == Constants.GameMode.Online)
                {
                    await Client.GameClient.Send(new Ok());
                }
                break;
            }
            case Constants.GameStatus.WaitAssertion:
                if (SharedRefs.Mode == Constants.GameMode.Online && _gameStates.MyTurn)
                {
                    await Client.GameClient.Receive(); // ACTION
                }
                _gameStates.GameStatus = Constants.GameStatus.SelectMyFish;
                break;
            case Constants.GameStatus.SelectMyFish:
                _gameStates.GameStatus = Constants.GameStatus.SelectEnemyFish;
                break;
            case Constants.GameStatus.SelectEnemyFish:
            {
                _gameStates.GameStatus = Constants.GameStatus.WaitingAnimation;
                if (SharedRefs.Mode == Constants.GameMode.Online && _gameStates.MyTurn)
                {
                    if (_gameStates.NormalAttack)
                    {
                        var enemyPos = 0;
                        for (var i = 0; i < 4; i++)
                        {
                            // ReSharper disable once InvertIf
                            if (_gameStates.EnemyFishSelectedAsTarget[i])
                            {
                                enemyPos = i;
                                break;
                            }
                        }
                        await Client.GameClient.Send(new NormalAction
                        {
                            MyPos = _gameStates.MyFishSelected,
                            EnemyPos = enemyPos
                        });
                    }
                    else
                    {
                        var myList = new List<int>();
                        var enemyList = new List<int>();
                        for (var i = 0; i < 4; i++)
                        {
                            if (_gameStates.MyFishSelectedAsTarget[i]) myList.Add(i);
                            if (_gameStates.EnemyFishSelectedAsTarget[i]) enemyList.Add(i);
                        }
                        await Client.GameClient.Send(new SkillAction
                        {
                            MyPos = _gameStates.MyFishSelected,
                            EnemyList = enemyList,
                            MyList = myList
                        });
                    }
                    var result = await Client.GameClient.Receive(); // SUCCESS/FAIL
                    if ((string) result["Action"] == "Success")
                    {
                        await Client.GameClient.Send(new Ok());
                    }
                    else
                    {
                        _gameStates.MyFishSelected = -1;
                        _gameStates.EnemyFishSelected = -1;
                        for (var i = 0; i < 4; i++)
                            _gameStates.MyFishSelectedAsTarget[i] = _gameStates.EnemyFishSelectedAsTarget[i] = false;
                        _gameStates.GameStatus = Constants.GameStatus.SelectMyFish;
                        return;
                    }
                }
                _gameStates.PassiveList.Clear();
                var enemy = _gameStates.EnemyFishSelected >= 0 && _gameStates.EnemyFishSelected < 4;
                if (_gameStates.NormalAttack)
                {
                    var selected = enemy ? _gameStates.EnemyFishSelected : _gameStates.MyFishSelected;
                    var target = 0;
                    for (var i = 0; i < 4; i++)
                    {
                        // ReSharper disable once InvertIf
                        if (enemy && _gameStates.MyFishSelectedAsTarget[i] || _gameStates.EnemyFishSelectedAsTarget[i])
                        {
                            target = i;
                            break;
                        }
                    }
                    var distance =
                        GameObjectManager.FishRelativePosition(enemy, selected) -
                        GameObjectManager.FishRelativePosition(!enemy, target);
                    Repeat(cnt =>
                    {
                        (enemy ? _gom.EnemyFishTransforms : _gom.MyFishTransforms)[selected].localPosition =
                            GameObjectManager.FishRelativePosition(!enemy, target) +
                            Math.Abs(cnt - 40f) / 40f * distance;
                    }, () => { }, 81, 0, 10);
                    _gameStates.PassiveList.Add(target);
                }
                else
                {
                    // var attackerTransforms = enemy ? _gom.EnemyFishTransforms : _gom.MyFishTransforms;
                    // var attackeeTransforms = enemy ? _gom.MyFishTransforms : _gom.EnemyFishTransforms;
                    var attackerSelected =
                        enemy ? _gameStates.EnemyFishSelectedAsTarget : _gameStates.MyFishSelectedAsTarget;
                    var attackeeSelected =
                        enemy ? _gameStates.MyFishSelectedAsTarget : _gameStates.EnemyFishSelectedAsTarget;
                    var attacker = enemy ? _gameStates.EnemyFishSelected : _gameStates.MyFishSelected;
                    switch (enemy
                        ? _gameStates.EnemyFishId[_gameStates.EnemyFishSelected]
                        : _gameStates.MyFishId[_gameStates.MyFishSelected])
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
                                    var originalDistance =
                                        GameObjectManager.FishRelativePosition(!enemy, id) -
                                        GameObjectManager.FishRelativePosition(enemy, attacker);
                                    var distance = originalDistance.x < 0
                                        ? originalDistance + new Vector3(4.5f, 0, 0)
                                        : originalDistance - new Vector3(4.5f, 0, 0);
                                    Instantiate(
                                        waterProjectile,
                                        GameObjectManager.FishRelativePosition(enemy, attacker) +
                                        new Vector3(3, 0, 0) * (enemy ? -1 : 1),
                                        Quaternion.Euler(
                                            new Vector3(0,
                                                Convert.ToInt32(Math.Atan(distance.x / distance.z) / Math.PI * 180.0),
                                                0
                                            )
                                        )
                                    );
                                }, cnt * 120);
                                _gameStates.PassiveList.Add(i);
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
                            myFishExplode.localPosition = GameObjectManager.FishRelativePosition(enemy, poorFish);
                            SetTimeout(() => { Destroy(myFishExplode.gameObject); }, 2000);
                            var myFishRecover = Instantiate(recoverEffect, allFishRoot);
                            myFishRecover.localPosition = GameObjectManager.FishRelativePosition(enemy, attacker);
                            SetTimeout(() => { Destroy(myFishRecover.gameObject); }, 4000);
                            break;
                        case 4:
                        case 6:
                        case 8:
                        case 9:
                            for (var i = 0; i < 4; i++)
                            {
                                // ReSharper disable once InvertIf
                                if (enemy && _gameStates.MyFishSelectedAsTarget[i] ||
                                    _gameStates.EnemyFishSelectedAsTarget[i])
                                {
                                    var target = i;
                                    var distance =
                                        GameObjectManager.FishRelativePosition(enemy, attacker) -
                                        GameObjectManager.FishRelativePosition(!enemy, target);
                                    Repeat(cnt =>
                                    {
                                        (enemy ? _gom.EnemyFishTransforms : _gom.MyFishTransforms)[attacker]
                                            .localPosition =
                                            GameObjectManager.FishRelativePosition(!enemy, target) +
                                            Math.Abs(cnt - 20f) / 20f * distance;
                                    }, () => { }, 41, 0, 10);
                                    SetTimeout(() =>
                                    {
                                        var targetExplode = Instantiate(explodePrefab, allFishRoot);
                                        targetExplode.localPosition =
                                            GameObjectManager.FishRelativePosition(!enemy, target);
                                        SetTimeout(() => { Destroy(targetExplode.gameObject); }, 1000);
                                    }, 200);
                                    _gameStates.PassiveList.Add(i);
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
                            shield.localPosition = GameObjectManager.FishRelativePosition(enemy, friendId);
                            SetTimeout(() => { Destroy(shield.gameObject); }, 5000);

                            var myselfRecover = Instantiate(recoverEffect, allFishRoot);
                            myselfRecover.localPosition = GameObjectManager.FishRelativePosition(enemy, attacker);
                            SetTimeout(() => { Destroy(myselfRecover.gameObject); }, 4000);
                            break;
                    }
                }
                SetTimeout(NewRound, _gameStates.PassiveList.Count > 0 ? 1100 : 1000);
                /* _gameStates.PassiveList.ForEach((id) =>
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
            case Constants.GameStatus.WaitingAnimation:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void Awake()
    {
        if (SharedRefs.Mode == Constants.GameMode.Offline)
        {
            var pickFish = SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["operation"][0]["Fish"];
            var next = SharedRefs.ReplayJson[SharedRefs.ReplayCursor + 1];
            for (var i = 0; i < 4; i++)
            {
                _gameStates.MyFishId[i] = (int) pickFish[0][i]["id"] - 1;
                _gameStates.EnemyFishId[i] = (int) pickFish[1][i]["id"] - 1;
                _gameStates.MyFishFullHp[i] = (int) next["players"][0]["fight_fish"][i]["hp"];
                _gameStates.EnemyFishFullHp[i] = (int) next["players"][1]["fight_fish"][i]["hp"];
            }
            _gom.Init(unkFishPrefab, allFishRoot);
            SharedRefs.ReplayCursor++;
        }
        for (var i = 0; i < 4; i++)
        {
            var myStatus = Instantiate(statusBarPrefab, myStatusRoot);
            myStatus.localPosition = new Vector3(10, -50 * i - 10);
            _gom.MyStatus.Add(myStatus.GetComponent<Slider>());
            var enemyStatus = Instantiate(statusBarPrefab, enemyStatusRoot);
            enemyStatus.localPosition = new Vector3(10, -50 * i - 10);
            _gom.EnemyStatus.Add(enemyStatus.GetComponent<Slider>());

            _gom.MyQuestions.Add(Instantiate(
                questionPrefab, GameObjectManager.FishRelativePosition(false, i) + new Vector3(0, 4, 0),
                Quaternion.Euler(new Vector3(0, -Convert.ToInt32(Math.Atan(3.0 * (i + 1) / (17 - i))), 0)),
                allFishRoot
            ));
            _gom.EnemyQuestions.Add(Instantiate(
                questionPrefab, GameObjectManager.FishRelativePosition(true, i) + new Vector3(0, 4, 0),
                Quaternion.Euler(new Vector3(0, Convert.ToInt32(Math.Atan(3.0 * (i + 1) / (17 - i))), 0)),
                allFishRoot)
            );
        }

        _dissolveShaderProperty = Shader.PropertyToID("_cutoff");

        if (SharedRefs.Mode == Constants.GameMode.Offline)
        {
            ProcessOffline();
        }
        else
        {
            _gameStates.GameStatus = Constants.GameStatus.WaitingAnimation;
            Task.Run(NewRound);
        }
    }

    protected override void RunPerFrame()
    {
        if (_gom.Initialized && _gameStates.GameStatus != Constants.GameStatus.WaitAssertion)
        {
            for (var i = 0; i < 4; i++)
            {
                if (_gameStates.MyFishAlive[i])
                {
                    if (_gameStates.MyFishSelectedAsTarget[i])
                        _gom.MyFishTransforms[i].localScale = _gom.Large;
                    else if (_gameStates.MyFishSelected == i)
                        _gom.MyFishTransforms[i].localScale = _gom.Large;
                    else
                        _gom.MyFishTransforms[i].localScale = _gom.Small;
                }

                // ReSharper disable once InvertIf
                if (_gameStates.EnemyFishAlive[i])
                {
                    if (_gameStates.EnemyFishSelectedAsTarget[i])
                        _gom.EnemyFishTransforms[i].localScale = _gom.Large;
                    else if (_gameStates.EnemyFishSelected == i)
                        _gom.EnemyFishTransforms[i].localScale = _gom.Large;
                    else
                        _gom.EnemyFishTransforms[i].localScale = _gom.Small;
                }
            }
        }

        changeStatusButton.interactable =
            _gameStates.GameStatus == Constants.GameStatus.DoAssertion ||
            _gameStates.GameStatus == Constants.GameStatus.SelectMyFish && _gameStates.MyFishSelected != -1 ||
            _gameStates.GameStatus == Constants.GameStatus.SelectEnemyFish &&
            (_gameStates.MyFishSelectedAsTarget.Any(b => b) ||
             _gameStates.EnemyFishSelectedAsTarget.Any(b => b));

        string title;
        switch (_gameStates.GameStatus)
        {
            case Constants.GameStatus.DoAssertion:
                title = _gameStates.Assertion == -1 ? "放弃断言" : "进行断言";
                break;
            case Constants.GameStatus.WaitAssertion:
                title = "请等待动画放完";
                break;
            case Constants.GameStatus.SelectMyFish:
                title = "选择我方鱼";
                break;
            case Constants.GameStatus.SelectEnemyFish:
                title = "选择作用对象";
                break;
            case Constants.GameStatus.WaitingAnimation:
                title = "请等待动画放完";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        changeStatusPrompt.text = title;

        (_gameStates.NormalAttack ? normalButton : skillButton).color = Color.green;
        (_gameStates.NormalAttack ? skillButton : normalButton).color = Color.blue;
    }

    public GameUI()
    {
        _gameStates = new GameStates();
        _gom = new GameObjectManager(_gameStates, Instantiate);
    }
}