using UnityEngine.SceneManagement;
using Utils;

namespace GameImpl
{
    public static class GameMoveCursorImpl
    {
        /// <summary>
        ///   <remarks>OFFLINE ONLY!</remarks>
        ///   <para>The method basically reads one record of replay data, handles
        /// it and increment the cursor by one.</para>
        /// </summary>
        public static void MoveCursor(this GameUI gameUI)
        {
            var state = SharedRefs.ReplayJson[SharedRefs.ReplayCursor];
            switch ((int) state["gamestate"])
            {
                case 2: // Current round is over, go back to preparation
                    SceneManager.LoadScene("Scenes/Preparation");
                    break;
                case 3: // Process Assertion
                {
                    SharedRefs.ReplayCursor++;
                    var operation = state["operation"][0];
                    if ((string) operation["Action"] == "Assert")
                    {
                        gameUI.GameState.AssertionPlayer = (int) operation["ID"];
                        gameUI.GameState.Assertion = (int) operation["Pos"];
                        gameUI.GameState.AssertionTarget = (int) operation["id"] - 1;
                        gameUI.MakeAGuess(gameUI.GameState.AssertionPlayer == 0, 2000);
                    }
                    else
                    {
                        gameUI.GameState.Assertion = -1;
                        gameUI.ChangeStatus();
                    }
                    break;
                }
                case 4: // Process Action
                {
                    SharedRefs.ReplayCursor++;
                    var operation = state["operation"][0];
                    if ((string) operation["Action"] == "Action")
                    {
                        // Set attacher
                        gameUI.GameState.MyTurn = (int) operation["ID"] == 0;
                        if (gameUI.GameState.MyTurn) gameUI.GameState.MyFishSelected = (int) operation["MyPos"];
                        else gameUI.GameState.EnemyFishSelected = (int) operation["MyPos"];
                        gameUI.ChangeStatus();

                        // Set attackee
                        var enemyListRef = gameUI.GameState.MyTurn
                            ? gameUI.GameState.EnemyFishSelectedAsTarget
                            : gameUI.GameState.MyFishSelectedAsTarget;
                        if (operation.ContainsKey("EnemyPos"))
                        {
                            enemyListRef[(int) operation["EnemyPos"]] = true;
                            gameUI.GameState.NormalAttack = true;
                        }
                        else
                        {
                            var enemyList = operation["EnemyList"];
                            for (var i = 0; i < enemyList.Count; i++)
                                enemyListRef[(int) enemyList[i]] = true;
                            gameUI.GameState.NormalAttack = false;
                        }
                        gameUI.ChangeStatus();
                    }

                    // Process hp and death
                    if (SharedRefs.ReplayJson[SharedRefs.ReplayCursor] != null)
                    {
                        var players = SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["players"];
                        var lastPlayers = SharedRefs.ReplayJson[SharedRefs.ReplayCursor - 2]["players"];
                        for (var i = 0; i < 4; i++)
                        {
                            if ((float) players[0]["fight_fish"][i]["hp"] <= 0 &&
                                (float) lastPlayers[0]["fight_fish"][i]["hp"] > 0)
                                gameUI.Dissolve(false, i);
                            if ((float) players[1]["fight_fish"][i]["hp"] <= 0 &&
                                (float) lastPlayers[1]["fight_fish"][i]["hp"] > 0)
                                gameUI.Dissolve(true, i);
                        }
                        gameUI.DisplayHpOffline(players);
                        gameUI.SetTimeout(gameUI.MoveCursor, 3000);
                    }
                    break;
                }
                default: // Something should be wrong
                    SharedRefs.ReplayCursor++;
                    gameUI.SetTimeout(gameUI.MoveCursor, 100);
                    break;
            }
        }
    }
}