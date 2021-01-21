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
            gameUI.AddLog("断言" + (hit ? "成功。" : "失败。"));
            if (hit)
            {
                Object.Destroy((gameUI.GameState.AssertionPlayer == 1
                    ? gameUI.Gom.MyFogs
                    : gameUI.Gom.EnemyFogs)[gameUI.GameState.Assertion].gameObject);
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
                        gameUI
                    );
                    gameUI.enemyExtensions[gameUI.GameState.Assertion].UpdateText(
                        $"{Constants.FishName[gameUI.GameState.AssertionTarget]}\n主动：{Constants.SkillDescription[gameUI.GameState.AssertionTarget]}\n被动：{Constants.PassiveDescription[gameUI.GameState.AssertionTarget]}"
                    );
                    gameUI.enemyProfiles[gameUI.GameState.Assertion].SetupFish(
                        gameUI.GameState.AssertionTarget,
                        gameUI.enemyExtensions[gameUI.GameState.Assertion]
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
                    if (SharedRefs.Mode == Constants.GameMode.Offline || !gameUI.GameState.MyTurn)
                        ((gameUI.GameState.AssertionPlayer == 1) ^ hit ? gameUI.enemyStatus : gameUI.myStatus)
                            [i].Current -= 50;
                }
        }
    }
}