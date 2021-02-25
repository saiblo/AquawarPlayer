using GameHelper;
using LitJson;
using UnityEngine;
using Utils;

namespace GameAnim
{
    public static class GamePassiveAnim
    {
        public static void PassiveAnim(this GameUI gameUI, JsonData actionInfo)
        {
            var passiveList = actionInfo["passive"];
            for (var i = 0; i < passiveList.Count; i++)
            {
                var sourcePos = (int) passiveList[i]["source"];
                var enemy = (int) passiveList[i]["player"] == 1;
                var value = (double) passiveList[i]["value"];
                var fishName =
                    Constants.FishName[(enemy ? gameUI.GameState.EnemyFishId : gameUI.GameState.MyFishId)[sourcePos]];
                var template = $"{(enemy ? 1 : 0)}号AI的{fishName}使用了被动技能：";
                switch ((string) passiveList[i]["type"])
                {
                    case "counter":
                    {
                        (enemy ? gameUI.GameState.EnemyUsedPassives : gameUI.GameState.MyUsedPassives)
                            [sourcePos].Add("膨胀反伤");
                        gameUI.SetTimeout(() =>
                        {
                            var explosion = Object.Instantiate(gameUI.smallExplosion, gameUI.allFishRoot);
                            explosion.localPosition = GameObjectManager.FishRelativePosition(enemy, sourcePos);
                            gameUI.SetTimeout(() => { Object.Destroy(explosion.gameObject); }, 1800);
                        }, 500);
                        gameUI.AddLog($"{template}膨胀反伤。");
                        break;
                    }
                    case "deflect":
                        (enemy ? gameUI.GameState.EnemyUsedPassives : gameUI.GameState.MyUsedPassives)
                            [sourcePos].Add("队友承伤");
                        gameUI.SetTimeout(() =>
                        {
                            for (var j = 0; j < 4; j++)
                            {
                                if (j == sourcePos) continue;
                                if (enemy && gameUI.enemyStatus[j].Current <= 0 ||
                                    !enemy && gameUI.myStatus[j].Current <= 0) continue;
                                var targetExplode = Object.Instantiate(gameUI.explodePrefab, gameUI.allFishRoot);
                                targetExplode.localPosition = GameObjectManager.FishRelativePosition(enemy, j);
                                gameUI.SetTimeout(() => { Object.Destroy(targetExplode.gameObject); }, 2000);
                            }
                        }, 400);
                        gameUI.AddLog($"{template}队友承伤。");
                        break;
                    case "reduce":
                    {
                        var name = value == 0 ? "闪避" : "减伤";
                        (enemy ? gameUI.GameState.EnemyUsedPassives : gameUI.GameState.MyUsedPassives)
                            [sourcePos].Add(name);
                        var shield = Object.Instantiate(gameUI.shieldEffect, gameUI.allFishRoot);
                        shield.localPosition = GameObjectManager.FishRelativePosition(enemy, sourcePos);
                        gameUI.SetTimeout(() => { Object.Destroy(shield.gameObject); }, 3000);
                        gameUI.AddLog($"{template}{name}。");
                        break;
                    }
                    case "heal":
                    {
                        (enemy ? gameUI.GameState.EnemyUsedPassives : gameUI.GameState.MyUsedPassives)
                            [sourcePos].Add("自愈");
                        gameUI.SetTimeout(() =>
                        {
                            var recover = Object.Instantiate(gameUI.recoverEffect, gameUI.allFishRoot);
                            recover.localPosition = GameObjectManager.FishRelativePosition(enemy, sourcePos);
                            gameUI.SetTimeout(() => { Object.Destroy(recover.gameObject); }, 2400);
                        }, 600);
                        gameUI.AddLog($"{template}自愈。");
                        break;
                    }
                    case "explode":
                    {
                        (enemy ? gameUI.GameState.EnemyUsedPassives : gameUI.GameState.MyUsedPassives)
                            [sourcePos].Add("亡语");
                        gameUI.SetTimeout(() =>
                        {
                            var fireBall = Object.Instantiate(gameUI.fireBallPrefab, gameUI.allFishRoot);
                            fireBall.localPosition = GameObjectManager.FishRelativePosition(enemy, sourcePos);
                            gameUI.SetTimeout(() => { Object.Destroy(fireBall.gameObject); }, 3000);
                        }, 500);
                        gameUI.AddLog($"{template}亡语。");
                        break;
                    }
                }
            }
        }
    }
}