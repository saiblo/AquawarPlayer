using System.Collections.Generic;
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
                    gameUI.resultText.gameObject.SetActive(true);
                    gameUI.resultText.text = "需与逻辑商议获胜";
                    gameUI.doneNextRoundButton.gameObject.SetActive(true);
                    break;
                case 3: // Process Assertion
                {
                    SharedRefs.ReplayCursor++;
                    var operation = state["operation"][0];
                    var subject = (int) state["cur_turn"] == 0 ? "我方" : "敌方";
                    var target = (int) state["cur_turn"] == 0 ? "敌方" : "我方";
                    if ((string) operation["Action"] == "Assert")
                    {
                        gameUI.GameState.AssertionPlayer = (int) operation["ID"];
                        gameUI.GameState.Assertion = (int) operation["Pos"];
                        gameUI.GameState.AssertionTarget = (int) operation["id"] - 1;
                        gameUI.MakeAGuess(gameUI.GameState.AssertionPlayer == 0, 2000);
                        gameUI.GameState.Logs.Enqueue(
                            $"{subject}断言{target}{operation["Pos"]}号位置鱼为{Constants.FishName[(int) operation["id"] - 1]}。"
                        );
                    }
                    else
                    {
                        gameUI.GameState.Assertion = -1;
                        gameUI.ChangeStatus();
                        gameUI.GameState.Logs.Enqueue($"{subject}放弃断言。");
                    }
                    break;
                }
                case 4: // Process Action
                {
                    SharedRefs.ReplayCursor++;
                    var operation = state["operation"][0];
                    var subject = (int) state["cur_turn"] == 0 ? "我方" : "敌方";
                    var target = (int) state["cur_turn"] == 0 ? "敌方" : "我方";
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
                        var attackerList = gameUI.GameState.MyTurn
                            ? gameUI.GameState.MyFishId
                            : gameUI.GameState.EnemyFishId;
                        var attackeeList = gameUI.GameState.MyTurn
                            ? gameUI.GameState.EnemyFishId
                            : gameUI.GameState.MyFishId;
                        if (operation.ContainsKey("EnemyPos"))
                        {
                            enemyListRef[(int) operation["EnemyPos"]] = true;
                            gameUI.GameState.NormalAttack = true;
                            gameUI.GameState.Logs.Enqueue(
                                $"{subject}{operation["MyPos"]}号位置的{Constants.FishName[attackerList[(int) operation["MyPos"]]]}对{target}{operation["EnemyPos"]}号位置的{Constants.FishName[attackeeList[(int) operation["MyPos"]]]}发动了普通攻击。"
                            );
                        }
                        else
                        {
                            var enemyList = operation["EnemyList"];
                            var enemyArray = new List<int>();
                            var enemyNames = new List<string>();
                            for (var i = 0; i < enemyList.Count; i++)
                            {
                                enemyListRef[(int) enemyList[i]] = true;
                                enemyArray.Add((int) enemyList[i]);
                                enemyNames.Add(Constants.FishName[attackeeList[(int) enemyList[i]]]);
                            }
                            gameUI.GameState.NormalAttack = false;
                            gameUI.GameState.Logs.Enqueue(
                                $"{subject}{operation["MyPos"]}号位置的{Constants.FishName[attackerList[(int) operation["MyPos"]]]}对{target}{string.Join(",", enemyArray)}号位置的{string.Join("、", enemyNames)}发动了主动技能。"
                            );
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
                        gameUI.UpdateFishStatus(players);
                        gameUI.SetTimeout(() =>
                        {
                            if (SharedRefs.AutoPlay)
                            {
                                gameUI.MoveCursor();
                            }
                            else
                            {
                                gameUI.nextStepButton.interactable = true;
                                gameUI.prevStepButton.interactable = true;
                            }
                        }, 3000);
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