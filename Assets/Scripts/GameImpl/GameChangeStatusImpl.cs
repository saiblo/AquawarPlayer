using System;
using System.Collections.Generic;
using GameAnim;
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
                    gameUI.GameState.GameStatus = Constants.GameStatus.WaitAssertion;

                    // When online and my turn, I have to make an assertion and see
                    // whether my assertion was correct. When online but not my turn,
                    // the assertion part has been handled in `NewRound`, so no
                    // operation is needed here.
                    if (SharedRefs.Mode == Constants.GameMode.Online && gameUI.GameState.MyTurn)
                    {
                        gameUI.assertionButtons.SetActive(false);
                        if (gameUI.GameState.Assertion == -1)
                            await SharedRefs.GameClient.Send(new Null());
                        else
                            await SharedRefs.GameClient.Send(
                                new Assert {Pos = gameUI.GameState.Assertion, ID = 0}
                            );
                        var reply = await SharedRefs.GameClient.Receive(); // ACTION
                        gameUI.GameState.AssertionPlayer = 0;
                        gameUI.GameState.OnlineAssertionHit = (bool) (reply["AssertReply"]["AssertResult"] ?? false);
                    }

                    // When either side made an assertion, play the animation.
                    if (gameUI.GameState.Assertion != -1)
                        gameUI.AssertionAnim();

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
                    else if (!gameUI.GameState.MyTurn)
                    {
                        var actionInfo = (await SharedRefs.GameClient.Receive())["ActionInfo"]; // SUCCESS
                        await SharedRefs.GameClient.Send(new Ok());

                        gameUI.ProcessActionInfo(actionInfo);

                        gameUI.RunOnUiThread(() =>
                        {
                            gameUI.ChangeStatus();
                            gameUI.ChangeStatus();
                            gameUI.DisplayHpOnline();
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
                        var result = await SharedRefs.GameClient.Receive(); // ASSERT
                        gameUI.ProcessActionInfo(result["MyAction"]);
                    }

                    // And now the animation part
                    gameUI.ActionAnim();

                    // Now go for a new round
                    gameUI.SetTimeout(gameUI.NewRound, gameUI.GameState.PassiveList.Count > 0 ? 1100 : 1000);
                    break;
                }
                case Constants.GameStatus.WaitingAnimation:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}