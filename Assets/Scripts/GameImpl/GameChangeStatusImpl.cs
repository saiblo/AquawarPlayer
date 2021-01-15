using System;
using System.Collections.Generic;
using GameAnim;
using GameHelper;
using UnityEngine.UI;
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
        public static async void ChangeStatus(this GameUI gameUI)
        {
            switch (gameUI.GameState.GameStatus)
            {
                case Constants.GameStatus.DoAssertion:
                {
                    var end = false;
                    gameUI.GameState.GameStatus = Constants.GameStatus.WaitAssertion;

                    // When online and my turn, I have to make an assertion and see
                    // whether my assertion was correct. When online but not my turn,
                    // the assertion result is stored in `SharedRefs.ActionInfo`, so
                    // no operation is needed here.
                    if (SharedRefs.Mode == Constants.GameMode.Online && gameUI.GameState.MyTurn)
                    {
                        gameUI.assertionButtons.SetActive(false);
                        if (gameUI.GameState.Assertion == -1)
                        {
                            await SharedRefs.GameClient.Send(new Null());
                            gameUI.GameState.Logs.Enqueue("我方放弃断言。");
                        }
                        else
                        {
                            await SharedRefs.GameClient.Send(new Assert
                                {
                                    Pos = gameUI.GameState.Assertion,
                                    ID = gameUI.GameState.AssertionTarget + 1
                                }
                            );
                            gameUI.GameState.Logs.Enqueue(
                                $"我方断言敌方{gameUI.GameState.Assertion}号位置的鱼为{Constants.FishName[gameUI.GameState.AssertionTarget]}。"
                            );
                        }
                        var reply = await SharedRefs.GameClient.Receive(); // ACTION
                        if ((string) reply["Action"] == "Finish") // You assert your way to death
                        {
                            end = true;
                            gameUI.resultText.text = (string) reply["Result"] == "Win" ? "我方获胜" : "敌方获胜";
                            gameUI.GameOver();
                        }
                        else
                        {
                            var info = reply["GameInfo"];
                            for (var i = 0; i < 4; i++)
                            {
                                gameUI.myStatus[i].Current = (int) info["MyHP"][i];
                                gameUI.enemyStatus[i].Current = (int) info["EnemyHP"][i];
                                gameUI.myProfiles[i].SetHp(gameUI.myStatus[i].Current);
                                gameUI.enemyProfiles[i].SetHp(gameUI.enemyStatus[i].Current);
                                gameUI.myProfiles[i].SetAtk((int) info["MyATK"][i]);
                            }
                        }
                        gameUI.GameState.AssertionPlayer = 0;
                        gameUI.GameState.OnlineAssertionHit = (bool) (reply["AssertReply"]["AssertResult"] ?? false);
                    }

                    // When either side made an assertion, play the animation.
                    if (gameUI.GameState.Assertion != -1) gameUI.AssertionAnim();

                    if (SharedRefs.Mode == Constants.GameMode.Online && !gameUI.GameState.MyTurn)
                    {
                        if (!SharedRefs.ActionInfo.ContainsKey("EnemyAction") ||
                            SharedRefs.ActionInfo["EnemyAction"] == null)
                        {
                            // Enemy asserted his way to death
                            end = true;
                            gameUI.resultText.text =
                                (string) SharedRefs.ActionInfo["Result"] == "Win" ? "我方获胜" : "敌方获胜";
                            gameUI.GameOver();
                        }
                        else
                        {
                            var info = SharedRefs.ActionInfo["GameInfo"];
                            for (var i = 0; i < 4; i++)
                            {
                                gameUI.myStatus[i].Current = (int) info["MyHP"][i];
                                gameUI.enemyStatus[i].Current = (int) info["EnemyHP"][i];
                                gameUI.myProfiles[i].SetHp(gameUI.myStatus[i].Current);
                                gameUI.enemyProfiles[i].SetHp(gameUI.enemyStatus[i].Current);
                            }
                        }
                    }

                    if (end) break;

                    // Enter `WaitAssertion` branch
                    gameUI.SetTimeout(() =>
                    {
                        gameUI.GameState.Assertion = -1;
                        gameUI.ChangeStatus();
                    }, gameUI.GameState.Assertion == -1 ? 500 : 1000);
                    break;
                }
                case Constants.GameStatus.WaitAssertion:
                    gameUI.GameState.GameStatus = Constants.GameStatus.SelectMyFish;
                    if (SharedRefs.Mode == Constants.GameMode.Offline)
                    {
                        gameUI.MoveCursor();
                    }
                    else if (gameUI.GameState.MyTurn)
                    {
                        gameUI.normalAttackButton.interactable = false;
                        gameUI.skillAttackButton.interactable = false;
                        gameUI.confirmAttackButton.interactable = false;
                        gameUI.normalAttackButton.GetComponent<Image>().overrideSprite = gameUI.lightBlue;
                        gameUI.skillAttackButton.GetComponent<Image>().overrideSprite = gameUI.lightBlue;
                        gameUI.attackButtons.SetActive(true);
                    }
                    else
                    {
                        gameUI.RunOnUiThread(() =>
                        {
                            gameUI.ProcessActionInfo(SharedRefs.ActionInfo["EnemyAction"]);
                            gameUI.ChangeStatus();
                            gameUI.ChangeStatus();
                        });
                    }
                    break;
                case Constants.GameStatus.SelectMyFish:
                    gameUI.GameState.GameStatus = Constants.GameStatus.SelectEnemyFish;
                    break;
                case Constants.GameStatus.SelectEnemyFish:
                {
                    gameUI.GameState.GameStatus = Constants.GameStatus.WaitingAnimation;

                    // Handle the communication part with remote
                    if (SharedRefs.Mode == Constants.GameMode.Online && gameUI.GameState.MyTurn)
                    {
                        gameUI.attackButtons.SetActive(false);
                        if (gameUI.GameState.NormalAttack)
                        {
                            var enemyPos = 0;
                            for (var i = 0; i < 4; i++)
                            {
                                // ReSharper disable once InvertIf
                                if (gameUI.GameState.EnemyFishSelectedAsTarget[i])
                                {
                                    enemyPos = i;
                                    break;
                                }
                            }
                            await SharedRefs.GameClient.Send(new NormalAction
                            {
                                MyPos = gameUI.GameState.MyFishSelected,
                                EnemyPos = enemyPos
                            });
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
                            await SharedRefs.GameClient.Send(new SkillAction
                            {
                                MyPos = gameUI.GameState.MyFishSelected,
                                EnemyList = enemyList,
                                MyList = myList
                            });
                        }
                        SharedRefs.ActionInfo = await SharedRefs.GameClient.Receive(); // ASSERT
                        gameUI.ProcessActionInfo(SharedRefs.ActionInfo["MyAction"]);
                    }

                    // And now the animation part
                    gameUI.ActionAnim();

                    if (SharedRefs.Mode == Constants.GameMode.Offline ||
                        !gameUI.GameState.MyTurn ||
                        SharedRefs.ActionInfo.ContainsKey("EnemyAssert"))
                    {
                        // Now go for a new round
                        gameUI.GameState.MyTurn = !gameUI.GameState.MyTurn;
                        gameUI.SetTimeout(gameUI.NewRound, gameUI.GameState.PassiveList.Count > 0 ? 1100 : 1000);
                    }
                    else
                    {
                        // Game over
                        gameUI.resultText.text = (string) SharedRefs.ActionInfo["Result"] == "Win" ? "我方获胜" : "敌方获胜";
                        gameUI.GameOver();
                    }
                    break;
                }
                case Constants.GameStatus.WaitingAnimation:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static async void GameOver(this GameBridge gameUI)
        {
            SharedRefs.PickInfo = await SharedRefs.GameClient.Receive(); // PICK
            gameUI.resultText.gameObject.SetActive(true);
            gameUI.doneNextRoundButton.gameObject.SetActive(true);
        }
    }
}