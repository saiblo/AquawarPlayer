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
                case 2: // Current round is over
                {
                    var gain = (int) SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["score"]
                               - (int) SharedRefs.ReplayJson[SharedRefs.ReplayCursor - 1]["score"];
                    gameUI.resultText.text = gain > 0 ? "我方获胜" : "敌方获胜";
                    gameUI.gameOverMask.SetActive(true);

                    for (var i = 0; i < 4; i++)
                        if ((int) SharedRefs.ReplayJson[SharedRefs.ReplayCursor - 1]["players"][(gain + 1) / 2]
                            ["fight_fish"][i]["state"] != 2)
                        {
                            (gain > 0 ? gameUI.enemyStatus : gameUI.myStatus)[i].Current = 0;
                            gameUI.Dissolve(gain > 0, i);
                        }

                    if ((int) SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["rounds"] == 3)
                        gameUI.gameOverText.text = "回到首页";
                    else
                        SharedRefs.ReplayCursor++;
                    break;
                }
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
                    if ((string) operation["Action"] == "Action")
                    {
                        // Set attacher
                        gameUI.GameState.MyTurn = (int) operation["ID"] == 0;
                        if (gameUI.GameState.MyTurn) gameUI.GameState.MyFishSelected = (int) operation["MyPos"];
                        else gameUI.GameState.EnemyFishSelected = (int) operation["MyPos"];
                        gameUI.ChangeStatus();

                        // Set attackee
                        if (operation.ContainsKey("EnemyPos"))
                        {
                            (gameUI.GameState.MyTurn
                                ? gameUI.GameState.EnemyFishSelectedAsTarget
                                : gameUI.GameState.MyFishSelectedAsTarget)[(int) operation["EnemyPos"]] = true;
                            gameUI.GameState.NormalAttack = true;
                        }
                        else if (operation["EnemyList"] != null)
                        {
                            gameUI.GameState.NormalAttack = false;
                        }
                        SharedRefs.ActionInfo = operation["ActionInfo"];
                        gameUI.ChangeStatus();
                    }

                    // Process hp and death
                    if (SharedRefs.ReplayJson[SharedRefs.ReplayCursor] != null)
                    {
                        var players = SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["players"];
                        var lastPlayers = SharedRefs.ReplayJson[SharedRefs.ReplayCursor - 2]["players"];
                        for (var i = 0; i < 4; i++)
                        {
                            if ((int) players[0]["fight_fish"][i]["state"] == 2 &&
                                (int) lastPlayers[0]["fight_fish"][i]["state"] != 2)
                                gameUI.Dissolve(false, i);
                            if ((int) players[1]["fight_fish"][i]["state"] == 2 &&
                                (int) lastPlayers[1]["fight_fish"][i]["state"] != 2)
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
                                gameUI.replayStepButton.interactable = true;
                                gameUI.prevRoundButton.interactable = true;
                                gameUI.nextRoundButton.interactable =
                                    (int) SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["rounds"] < 2;
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