using Utils;

namespace GameImpl
{
    public static class GameNewRoundImpl
    {
        /// <summary>
        ///   <para>First, resets the game states.<br/>
        /// After that, for online mode, listens to message from remote
        /// and determine what to show next; for offline mode, simply resets
        /// <code>_gameStatus</code> to <code>DoAssertion</code>.</para>
        /// </summary>
        public static void NewRound(this GameUI gameUI)
        {
            gameUI.GameState.MyFishSelected = -1;
            gameUI.GameState.EnemyFishSelected = -1;
            gameUI.GameState.NormalAttack = true;
            for (var i = 0; i < 4; i++)
                gameUI.GameState.MyFishSelectedAsTarget[i] = gameUI.GameState.EnemyFishSelectedAsTarget[i] = false;

            if (SharedRefs.Mode == Constants.GameMode.Offline)
            {
                gameUI.GameState.GameStatus = Constants.GameStatus.DoAssertion;
            }
            else
            {
                var gameInfo = SharedRefs.ActionInfo["GameInfo"];
                gameUI.GameState.GameStatus = Constants.GameStatus.DoAssertion;
                for (var i = 0; i < 4; i++)
                {
                    gameUI.GameState.MyFishId[i] = SharedRefs.FishChosen[i];
                    if ((int) gameInfo["EnemyFish"][i] > 0)
                        gameUI.GameState.EnemyFishId[i] = (int) gameInfo["EnemyFish"][i] - 1;
                    gameUI.myStatus[i].Current = (int) gameInfo["MyHP"][i];
                    gameUI.enemyStatus[i].Current = (int) gameInfo["EnemyHP"][i];
                    gameUI.myProfiles[i].SetHp(gameUI.myStatus[i].Current);
                    gameUI.enemyProfiles[i].SetHp(gameUI.enemyStatus[i].Current);
                    gameUI.myProfiles[i].SetAtk((int) gameInfo["MyATK"][i]);
                }

                if (!gameUI.Gom.Initialized) gameUI.Gom.Init(gameUI);

                if (gameUI.GameState.MyTurn)
                {
                    gameUI.Gom.ResetCountDown(gameUI);
                    return;
                }

                gameUI.GameState.AssertionPlayer = 1;
                var enemyAssert = SharedRefs.ActionInfo["EnemyAssert"];
                if (enemyAssert["AssertPos"] == null)
                {
                    gameUI.AddLog($"{GameUI.EnemyStr}放弃断言。");
                    gameUI.GameState.Assertion = -1;
                    gameUI.GameState.OnlineAssertionHit = false;
                    gameUI.GameState.AssertionTarget = 0;
                    gameUI.ChangeStatus(); // Skips the next two stages
                    gameUI.ChangeStatus();
                    return;
                }

                gameUI.GameState.Assertion = (int) enemyAssert["AssertPos"];
                gameUI.GameState.OnlineAssertionHit = (bool) enemyAssert["AssertResult"];
                gameUI.GameState.AssertionTarget = (int) enemyAssert["AssertContent"] - 1;
                gameUI.MakeAGuess(false, 1200);
                gameUI.SetTimeout(gameUI.ChangeStatus, 3000); // Just waits for the assertion animation to finish

                gameUI.AddLog(
                    $"{GameUI.EnemyStr}断言{GameUI.MeStr}{gameUI.GameState.Assertion}号位置的鱼为{Constants.FishName[gameUI.GameState.AssertionTarget]}。"
                );
            }
        }
    }
}