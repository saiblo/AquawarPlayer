using System;
using System.Collections.Generic;
using GameAnim;
using GameHelper;
using LitJson;
using Utils;

namespace GameImpl
{
    public static class GameChangeStatusImpl
    {
        /// <summary>
        ///   <para>Probably the most complicated part of the GameUI</para>
        ///   <para>Consider the game as a simple state machine. States are to
        /// be changed within enum <code>GameStatus</code>.<br/>
        ///   When This method is called, it indicates that there is a change
        /// of status in this game, and certain actions should be performed.</para>
        ///   <para>Basically, there are five states available:<br/>
        /// <code>DoAssertion</code><br/>
        /// <code>WaitAssertion</code><br/>
        /// <code>SelectMyFish</code><br/>
        /// <code>SelectEnemyFish</code><br/>
        /// <code>WaitingAnimation</code>
        /// </para>
        /// <para><code>SelectMyFish</code> and <code>WaitingAnimation</code>
        /// are rather simple - they merely serves as a no-op.</para>
        /// <para>As for `DoAssertion`, the main things that the method should
        /// handle are sending assertion selection to remote (when on player's
        /// turn) and playing assertion animation.</para>
        /// <para>And for `WaitAssertion`, literally it means waiting for the
        /// assertion animation to finish, and at this stage, this function
        /// shall either prepare the player for his operation when online and
        /// is his turn, or read the operation results from remote or replay.</para>
        /// <para>Lastly, for the `SelectEnemyFish` part, if it is the player's
        /// turn, send his attacking plan to remote first. After that, play the
        /// attacking animations.</para>
        /// </summary>
        public static void ChangeStatus(this GameUI gameUI)
        {
            switch (gameUI.GameState.GameStatus)
            {
                case Constants.GameStatus.DoAssertion:
                {
                    var end = false;
                    gameUI.GameState.GameStatus = Constants.GameStatus.WaitAssertion;

                    void AfterWorks()
                    {
                        gameUI.RunOnUiThread(() =>
                        {
                            // When either side made an assertion, play the animation.
                            if (gameUI.GameState.Assertion != -1) gameUI.AssertionAnim();

                            if (SharedRefs.Mode == Constants.GameMode.Online && !gameUI.GameState.MyTurn)
                            {
                                if ((string) SharedRefs.ActionInfo["Action"] == "Finish")
                                {
                                    end = true;
                                    gameUI.resultText.text =
                                        (string) SharedRefs.ActionInfo["Result"] == "Win"
                                            ? $"{GameUI.MeStr}获胜"
                                            : $"{GameUI.EnemyStr}获胜";
                                    gameUI.GameOver((string) SharedRefs.ActionInfo["Result"] == "Win");
                                }
                                else
                                {
                                    for (var i = 0; i < 4; i++)
                                    {
                                        var id = i;
                                        if (gameUI.GameState.MyFishAlive[i] && gameUI.myStatus[i].Current <= 0)
                                            gameUI.SetTimeout(() => { gameUI.Dissolve(false, id); }, 500);
                                        if (gameUI.GameState.EnemyFishAlive[i] && gameUI.enemyStatus[i].Current <= 0)
                                            gameUI.SetTimeout(() => { gameUI.Dissolve(true, id); }, 500);
                                    }
                                }
                            }

                            if (end) return;

                            // Enter `WaitAssertion` branch
                            gameUI.SetTimeout(() =>
                            {
                                gameUI.GameState.Assertion = -1;
                                gameUI.ChangeStatus();
                            }, gameUI.GameState.Assertion == -1 ? 200 : 1000);
                        });
                    }

                    // When online and my turn, I have to make an assertion and see
                    // whether my assertion was correct. When online but not my turn,
                    // the assertion result is stored in `SharedRefs.ActionInfo`, so
                    // no operation is needed here.
                    if (SharedRefs.Mode == Constants.GameMode.Online && gameUI.GameState.MyTurn)
                    {
                        if (gameUI.GameState.Assertion == -1)
                        {
                            GameUI.SendWsMessage(JsonMapper.ToJson(new Null()));
                            gameUI.AddLog($"{GameUI.MeStr}放弃断言。");
                        }
                        else
                        {
                            GameUI.SendWsMessage(JsonMapper.ToJson(new Assert
                                {
                                    Pos = gameUI.GameState.Assertion,
                                    ID = gameUI.GameState.AssertionTarget + 1
                                }
                            ));
                            gameUI.AddLog(
                                $"{GameUI.MeStr}断言{GameUI.EnemyStr}{gameUI.GameState.Assertion}号位置的鱼为{Constants.FishName[gameUI.GameState.AssertionTarget]}。"
                            );
                        }
                        SharedRefs.OnlineWaiting = 4;
                    }
                    else
                    {
                        AfterWorks();
                    }
                    break;
                }
                case Constants.GameStatus.WaitAssertion:
                    gameUI.GameState.GameStatus = Constants.GameStatus.SelectMyFish;
                    if (SharedRefs.Mode == Constants.GameMode.Offline)
                        gameUI.MoveCursor();
                    else if (gameUI.GameState.MyTurn)
                        gameUI.Gom.ResetCountDown(gameUI);
                    else
                        gameUI.RunOnUiThread(() =>
                        {
                            gameUI.ChangeStatus();
                            gameUI.ChangeStatus();
                        });
                    break;
                case Constants.GameStatus.SelectMyFish:
                    gameUI.GameState.GameStatus = Constants.GameStatus.SelectEnemyFish;
                    break;
                case Constants.GameStatus.SelectEnemyFish:
                {
                    gameUI.GameState.GameStatus = Constants.GameStatus.WaitingAnimation;
                    gameUI.AddLog();

                    void AfterWorks()
                    {
                        gameUI.RunOnUiThread(() =>
                        {
                            // And now the animation part
                            var hasPassive = gameUI.ActionAnim();

                            if (SharedRefs.Mode == Constants.GameMode.Offline ||
                                !gameUI.GameState.MyTurn ||
                                SharedRefs.ActionInfo.ContainsKey("EnemyAssert"))
                            {
                                // Now go for a new round
                                gameUI.GameState.MyTurn = !gameUI.GameState.MyTurn;
                                gameUI.SetTimeout(gameUI.NewRound, hasPassive ? 2000 : 1000);
                            }
                            else
                            {
                                // Game over
                                gameUI.resultText.text = (string) SharedRefs.ActionInfo["Result"] == "Win"
                                    ? $"{GameUI.MeStr}获胜"
                                    : $"{GameUI.EnemyStr}获胜";
                                gameUI.GameOver((string) SharedRefs.ActionInfo["Result"] == "Win");
                            }
                        });
                    }

                    // Handle the communication part with remote
                    if (SharedRefs.Mode == Constants.GameMode.Online && gameUI.GameState.MyTurn)
                    {
                        gameUI.Gom.StopCountDown(gameUI);
                        if (gameUI.GameState.NormalAttack)
                        {
                            var enemyPos = 0;
                            for (var i = 0; i < 4; i++)
                            {
                                if (!gameUI.GameState.EnemyFishSelectedAsTarget[i]) continue;
                                enemyPos = i;
                                break;
                            }
                            GameUI.SendWsMessage(JsonMapper.ToJson(new NormalAction
                            {
                                MyPos = gameUI.GameState.MyFishSelected,
                                EnemyPos = enemyPos
                            }));
                        }
                        else
                        {
                            var myList = new List<int>();
                            var enemyList = new List<int>();
                            for (var i = 0; i < 4; i++)
                            {
                                if (gameUI.GameState.MyFishSelectedAsTarget[i]) myList.Add(i);
                                if (gameUI.GameState.EnemyFishSelectedAsTarget[i]) enemyList.Add(i);
                            }
                            if (gameUI.GameState.MyFishId[gameUI.GameState.MyFishSelected] == 6)
                                gameUI.counters[gameUI.GameState.MyFishSelected].text =
                                    $"{++gameUI.GameState.TurtleUsed}";
                            if (gameUI.GameState.MyFishId[gameUI.GameState.MyFishSelected] == 10)
                                gameUI.counters[gameUI.GameState.MyFishSelected].text =
                                    $"{++gameUI.GameState.ClownUsed}";
                            if (gameUI.GameState.MyFishId[gameUI.GameState.MyFishSelected] == 11 &&
                                (SharedRefs.MyImitate == 6 || SharedRefs.MyImitate == 10))
                                gameUI.counters[gameUI.GameState.MyFishSelected].text =
                                    $"{++gameUI.GameState.ImitateUsed}";
                            GameUI.SendWsMessage(JsonMapper.ToJson(new SkillAction
                            {
                                MyPos = gameUI.GameState.MyFishSelected,
                                EnemyList = enemyList,
                                MyList = myList
                            }));
                        }
                        SharedRefs.OnlineWaiting = 3;
                    }
                    else
                    {
                        AfterWorks();
                    }
                    break;
                }
                case Constants.GameStatus.WaitingAnimation:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void GameOver(this GameBridge gameUI, bool win, bool force = false)
        {
            if (win) ++SharedRefs.OnlineWin;
            else ++SharedRefs.OnlineLose;
            if (SharedRefs.Mode == Constants.GameMode.Online)
                gameUI.scoreText.text = $"{SharedRefs.OnlineLose}:{SharedRefs.OnlineWin}";
            if (SharedRefs.OnlineLose + SharedRefs.OnlineWin != 3 && !force)
            {
                SharedRefs.OnlineWaiting = 5;
                return;
            }
            if (force)
            {
                SharedRefs.ErrorFlag = true;
                gameUI.gameOverText.text = "回到首页";
            }
            gameUI.gameOverMask.SetActive(true);
        }
    }
}