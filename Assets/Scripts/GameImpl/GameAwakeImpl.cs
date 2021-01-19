﻿using GameHelper;
using UnityEngine;
using Utils;
using Object = UnityEngine.Object;

namespace GameImpl
{
    public static class GameAwakeImpl
    {
        public static void AwakeImpl(this GameUI gameUI)
        {
            for (var i = 0; i < 4; i++)
            {
                gameUI.Gom.MyFogs.Add(
                    Object.Instantiate(
                        gameUI.fogPrefab,
                        GameObjectManager.FishRelativePosition(false, i),
                        Quaternion.identity,
                        gameUI.allFishRoot
                    )
                );
                gameUI.Gom.EnemyFogs.Add(
                    Object.Instantiate(
                        gameUI.fogPrefab,
                        GameObjectManager.FishRelativePosition(true, i),
                        Quaternion.identity,
                        gameUI.allFishRoot
                    )
                );
            }

            gameUI.DissolveShaderProperty = Shader.PropertyToID("_cutoff");

            if (SharedRefs.Mode == Constants.GameMode.Offline)
            {
                if (ErrorParser.HandleErrorCheck(gameUI)) return;
                var players = SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["players"];
                var pickFish = SharedRefs.ReplayJson[SharedRefs.ReplayCursor++]["operation"][0]["Fish"];
                for (var i = 0; i < 4; i++)
                {
                    var myFishId = (int) pickFish[0][i]["id"] - 1;
                    var enemyFishId = (int) pickFish[1][i]["id"] - 1;
                    gameUI.GameState.MyFishId[i] = myFishId;
                    gameUI.GameState.EnemyFishId[i] = enemyFishId;
                    gameUI.myStatus[i].Full = (int) pickFish[0][i]["hp"];
                    gameUI.enemyStatus[i].Full = (int) pickFish[1][i]["hp"];
                    gameUI.GameState.MyFishPicked.Add(gameUI.GameState.MyFishId[i]);
                    gameUI.GameState.EnemyFishPicked.Add(gameUI.GameState.EnemyFishId[i]);
                    if (pickFish[0][i].ContainsKey("imitate"))
                        SharedRefs.MyImitate = (int) pickFish[0][i]["imitate"] - 1;
                    if (pickFish[1][i].ContainsKey("imitate"))
                        SharedRefs.EnemyImitate = (int) pickFish[1][i]["imitate"] - 1;
                }
                for (var i = 0; i < players[0]["my_fish"].Count; i++)
                    gameUI.GameState.MyFishAvailable.Add((int) players[0]["my_fish"][i]["id"] - 1);
                for (var i = 0; i < players[1]["my_fish"].Count; i++)
                    gameUI.GameState.EnemyFishAvailable.Add((int) players[1]["my_fish"][i]["id"] - 1);

                var rounds = (int) SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["rounds"] + 1;
                var score = (int) SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["score"];
                gameUI.scoreText.text = $"{(rounds - score - 1) / 2}:{(score + rounds - 1) / 2}";
                gameUI.Gom.Init(gameUI);
                if (SharedRefs.AutoPlay) gameUI.MoveCursor();
                else
                {
                    gameUI.prevRoundButton.interactable = true;
                    gameUI.nextRoundButton.interactable = rounds < 3;
                    gameUI.nextStepButton.interactable = true;
                }
            }
            else
            {
                gameUI.GameState.GameStatus = Constants.GameStatus.WaitingAnimation;
                gameUI.RunOnUiThread(async () =>
                {
                    SharedRefs.ActionInfo = await SharedRefs.GameClient.Receive(); // ASSERT
                    gameUI.GameState.MyTurn = SharedRefs.ActionInfo["EnemyAction"] == null;
                    for (var i = 0; i < Constants.FishNum; i++)
                        gameUI.assertionModal.SetupFish(i, Constants.FishState.Free, gameUI.assertionExt, gameUI);
                    gameUI.NewRound();
                });
            }
        }
    }
}