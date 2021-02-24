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
            gameUI.GameState.GameStatus = Constants.GameStatus.DoAssertion;
            for (var i = 0; i < 4; i++)
                gameUI.GameState.MyFishSelectedAsTarget[i] = gameUI.GameState.EnemyFishSelectedAsTarget[i] = false;
            gameUI.AddLog();

            var players = SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["players"];
            for (var i = 0; i < 4; i++)
            {
                gameUI.myProfiles[i].SetAtk((int) players[0]["fight_fish"][i]["atk"]);
                gameUI.enemyProfiles[i].SetAtk((int) players[1]["fight_fish"][i]["atk"]);
            }
        }
    }
}