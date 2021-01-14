using System.Threading.Tasks;
using GameHelper;
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
                var players = SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["players"];
                var pickFish = SharedRefs.ReplayJson[SharedRefs.ReplayCursor++]["operation"][0]["Fish"];
                for (var i = 0; i < 4; i++)
                {
                    gameUI.GameState.MyFishId[i] = (int) pickFish[0][i]["id"] - 1;
                    gameUI.GameState.EnemyFishId[i] = (int) pickFish[1][i]["id"] - 1;
                    gameUI.myStatus[i].Full = (int) pickFish[0][i]["hp"];
                    gameUI.enemyStatus[i].Full = (int) pickFish[1][i]["hp"];
                    gameUI.GameState.MyFishPicked.Add(gameUI.GameState.MyFishId[i]);
                    gameUI.GameState.EnemyFishPicked.Add(gameUI.GameState.EnemyFishId[i]);
                }
                for (var i = 0; i < players[0]["my_fish"].Count; i++)
                    gameUI.GameState.MyFishAvailable.Add((int) players[0]["my_fish"][i]["id"] - 1);
                for (var i = 0; i < players[1]["my_fish"].Count; i++)
                    gameUI.GameState.EnemyFishAvailable.Add((int) players[1]["my_fish"][i]["id"] - 1);

                var rounds = (int) SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["rounds"] + 1;
                gameUI.roundText.text = $"回合数：{rounds}/3";
                gameUI.scoreText.text = $"我方得分：{(int) SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["score"]}";
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
                Task.Run(gameUI.NewRound);
            }
        }
    }
}