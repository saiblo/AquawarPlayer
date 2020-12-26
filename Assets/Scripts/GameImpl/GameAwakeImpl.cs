using System;
using System.Threading.Tasks;
using GameHelper;
using UnityEngine;
using UnityEngine.UI;
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
                var myStatus = Object.Instantiate(gameUI.statusBarPrefab, gameUI.myStatusRoot);
                myStatus.localPosition = new Vector3(10, -50 * i - 10);
                gameUI.Gom.MyStatus.Add(myStatus.GetComponent<Slider>());
                var enemyStatus = Object.Instantiate(gameUI.statusBarPrefab, gameUI.enemyStatusRoot);
                enemyStatus.localPosition = new Vector3(10, -50 * i - 10);
                gameUI.Gom.EnemyStatus.Add(enemyStatus.GetComponent<Slider>());

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

            gameUI.DissolveShaderProperty = Shader.PropertyToID("_cutoff");

            if (SharedRefs.Mode == Constants.GameMode.Offline)
            {
                var pickFish = SharedRefs.ReplayJson[SharedRefs.ReplayCursor++]["operation"][0]["Fish"];
                for (var i = 0; i < 4; i++)
                {
                    gameUI.GameState.MyFishId[i] = (int) pickFish[0][i]["id"] - 1;
                    gameUI.GameState.EnemyFishId[i] = (int) pickFish[1][i]["id"] - 1;
                    gameUI.GameState.MyFishFullHp[i] = (int) pickFish[0][i]["hp"];
                    gameUI.GameState.EnemyFishFullHp[i] = (int) pickFish[1][i]["hp"];
                }
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