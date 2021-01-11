using System;
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
                gameUI.Gom.MyQuestions.Add(Object.Instantiate(
                    gameUI.questionPrefab, GameObjectManager.FishRelativePosition(false, i) + new Vector3(0, 4, 0),
                    Quaternion.Euler(new Vector3(0, -Convert.ToInt32(Math.Atan(3.0 * (i + 1) / (17 - i))), 0)),
                    gameUI.allFishRoot
                ));
                gameUI.Gom.EnemyQuestions.Add(Object.Instantiate(
                    gameUI.questionPrefab, GameObjectManager.FishRelativePosition(true, i) + new Vector3(0, 4, 0),
                    Quaternion.Euler(new Vector3(0, Convert.ToInt32(Math.Atan(3.0 * (i + 1) / (17 - i))), 0)),
                    gameUI.allFishRoot)
                );
            }

            for (var i = 0; i < 4; i++)
            {
                gameUI.Gom.MyQuestions[i].gameObject.SetActive(false);
                gameUI.Gom.EnemyQuestions[i].gameObject.SetActive(false);
            }

            gameUI.DissolveShaderProperty = Shader.PropertyToID("_cutoff");

            if (SharedRefs.Mode == Constants.GameMode.Offline)
            {
                var pickFish = SharedRefs.ReplayJson[SharedRefs.ReplayCursor++]["operation"][0]["Fish"];
                for (var i = 0; i < 4; i++)
                {
                    gameUI.GameState.MyFishId[i] = (int) pickFish[0][i]["id"] - 1;
                    gameUI.GameState.EnemyFishId[i] = (int) pickFish[1][i]["id"] - 1;
                    gameUI.myStatus[i].Full = (int) pickFish[0][i]["hp"];
                    gameUI.enemyStatus[i].Full = (int) pickFish[1][i]["hp"];
                    gameUI.myProfiles[i].SetupFish(gameUI.GameState.MyFishId[i]);
                    gameUI.enemyProfiles[i].SetupFish(gameUI.GameState.EnemyFishId[i]);
                    gameUI.myProfiles[i].SetHp(gameUI.myStatus[i].Full);
                    gameUI.enemyProfiles[i].SetHp(gameUI.enemyStatus[i].Full);
                    gameUI.myProfiles[i].SetAtk((int) pickFish[0][i]["atk"]);
                    gameUI.enemyProfiles[i].SetAtk((int) pickFish[1][i]["atk"]);
                }
                gameUI.roundText.text = $"回合数：{(int) SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["rounds"] + 1}/3";
                gameUI.scoreText.text = $"我方得分：{(int) SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["score"]}";
                gameUI.resultText.gameObject.SetActive(false);
                gameUI.doneNextRoundButton.gameObject.SetActive(false);
                gameUI.logObject.SetActive(false);
                gameUI.Gom.Init(gameUI.unkFishPrefab, gameUI.allFishRoot);
                gameUI.MoveCursor();
            }
            else
            {
                gameUI.GameState.GameStatus = Constants.GameStatus.WaitingAnimation;
                Task.Run(gameUI.NewRound);
            }
        }
    }
}