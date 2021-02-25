using System;
using GameAnim;
using Utils;
using Object = UnityEngine.Object;

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
                    if (gameUI.GameState.Assertion == -1)
                    {
                        gameUI.GameState.GameStatus = Constants.GameStatus.WaitAssertion;
                        gameUI.SetTimeout(gameUI.ChangeStatus, 100);
                    }
                    else
                    {
                        gameUI.GameState.GameStatus = Constants.GameStatus.PeekAssertion;
                        SharedRefs.AutoPlay = false;
                        gameUI.nextStepButton.interactable = true;
                    }
                    break;
                }
                case Constants.GameStatus.PeekAssertion:
                    gameUI.GameState.GameStatus = Constants.GameStatus.WaitAssertion;
                    Object.Destroy(gameUI.Gom.GuessFish);
                    gameUI.AssertionAnim();
                    gameUI.GameState.Assertion = -1;
                    gameUI.SetTimeout(gameUI.ChangeStatus, 1000);
                    break;
                case Constants.GameStatus.WaitAssertion:
                    gameUI.GameState.GameStatus = Constants.GameStatus.SelectMyFish;
                    gameUI.MoveCursor();
                    break;
                case Constants.GameStatus.SelectMyFish:
                    gameUI.GameState.GameStatus = Constants.GameStatus.SelectEnemyFish;
                    break;
                case Constants.GameStatus.SelectEnemyFish:
                {
                    gameUI.GameState.GameStatus = Constants.GameStatus.WaitingAnimation;
                    gameUI.AddLog();

                    // And now the animation part
                    var hasPassive = gameUI.ActionAnim();

                    // Now go for a new round
                    gameUI.GameState.MyTurn = !gameUI.GameState.MyTurn;
                    gameUI.SetTimeout(gameUI.NewRound, hasPassive ? 2000 : 1000);
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