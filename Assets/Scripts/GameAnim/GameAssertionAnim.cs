using GameHelper;
using UnityEngine;
using Utils;

namespace GameAnim
{
    public static class GameAssertionAnim
    {
        public static void AssertionAnim(this GameUI gameUI)
        {
            var hit =
                SharedRefs.Mode == Constants.GameMode.Offline &&
                gameUI.GameState.AssertionTarget == (gameUI.GameState.AssertionPlayer == 1
                    ? gameUI.GameState.MyFishId
                    : gameUI.GameState.EnemyFishId)[gameUI.GameState.Assertion] ||
                SharedRefs.Mode == Constants.GameMode.Online && gameUI.GameState.OnlineAssertionHit;
            gameUI.GameState.Logs.Enqueue("断言" + (hit ? "成功。" : "失败。"));
            if (hit)
            {
                Object.Destroy((gameUI.GameState.AssertionPlayer == 1
                    ? gameUI.Gom.MyQuestions
                    : gameUI.Gom.EnemyQuestions)[gameUI.GameState.Assertion].gameObject);
                (gameUI.GameState.AssertionPlayer == 1
                    ? gameUI.GameState.MyFishExpose
                    : gameUI.GameState.EnemyFishExpose)[gameUI.GameState.Assertion] = true;

                if (SharedRefs.Mode == Constants.GameMode.Online)
                {
                    (gameUI.GameState.AssertionPlayer == 1
                            ? gameUI.GameState.MyFishId
                            : gameUI.GameState.EnemyFishId)
                        [gameUI.GameState.Assertion] = gameUI.GameState.AssertionTarget;
                    var transforms =
                        gameUI.GameState.AssertionPlayer == 1
                            ? gameUI.Gom.MyFishTransforms
                            : gameUI.Gom.EnemyFishTransforms;
                    Object.Destroy(transforms[gameUI.GameState.Assertion].gameObject);
                    transforms[gameUI.GameState.Assertion] = gameUI.Gom.GenFish(
                        gameUI.GameState.AssertionPlayer == 0,
                        gameUI.GameState.Assertion,
                        gameUI.unkFishPrefab,
                        gameUI.allFishRoot
                    );
                }
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
                }
        }
    }
}