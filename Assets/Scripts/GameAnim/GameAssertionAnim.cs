using GameHelper;
using UnityEngine;

namespace GameAnim
{
    public static class GameAssertionAnim
    {
        public static void AssertionAnim(this GameUI gameUI)
        {
            var hit =
                gameUI.GameState.AssertionTarget == (gameUI.GameState.AssertionPlayer == 1
                    ? gameUI.GameState.MyFishId
                    : gameUI.GameState.EnemyFishId)[gameUI.GameState.Assertion];
            gameUI.AddLog("断言" + (hit ? "成功。" : "失败。"));
            if (hit)
            {
                (gameUI.GameState.AssertionPlayer == 1
                    ? gameUI.Gom.MyFogs
                    : gameUI.Gom.EnemyFogs)[gameUI.GameState.Assertion].gameObject.SetActive(false);
                (gameUI.GameState.AssertionPlayer == 1
                    ? gameUI.GameState.MyFishExpose
                    : gameUI.GameState.EnemyFishExpose)[gameUI.GameState.Assertion] = true;
            }

            for (var i = 0; i < 4; i++)
                if (((gameUI.GameState.AssertionPlayer == 1) ^ hit
                    ? gameUI.GameState.EnemyFishAlive
                    : gameUI.GameState.MyFishAlive)[i])
                {
                    var explosionObj = Object.Instantiate(
                        gameUI.explodePrefab,
                        GameObjectManager.FishRelativePosition(
                            (gameUI.GameState.AssertionPlayer == 1) ^ hit, i),
                        Quaternion.identity,
                        gameUI.allFishRoot).gameObject;
                    gameUI.SetTimeout(() => { Object.Destroy(explosionObj); }, 2000);
                    ((gameUI.GameState.AssertionPlayer == 1) ^ hit ? gameUI.enemyStatus : gameUI.myStatus)
                        [i].Current -= 50;
                }
        }
    }
}