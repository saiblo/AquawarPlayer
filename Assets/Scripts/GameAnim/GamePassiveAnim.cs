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
                var enemy = SharedRefs.Mode == Constants.GameMode.Offline
                    ? (int) passiveList[i]["player"] == 1
                    : (bool) passiveList[i]["isEnemy"];
                var sourceName = enemy ? "敌方" : "我方";
                switch ((string) passiveList[i]["type"])
                {
                    case "counter":
                    {
                        gameUI.SetTimeout(() =>
                        {
                            var explosion = Object.Instantiate(gameUI.smallExplosion, gameUI.allFishRoot);
                            explosion.localPosition = GameObjectManager.FishRelativePosition(enemy, sourcePos);
                            gameUI.SetTimeout(() => { Object.Destroy(explosion.gameObject); }, 1800);
                        }, 500);
                        gameUI.GameState.Logs.Enqueue($"{sourceName}{sourcePos}号位置的鱼使用了被动技能：膨胀反伤。");
                        break;
                    }
                    case "deflect":
                        gameUI.SetTimeout(() =>
                        {
                            for (var j = 0; j < 4; j++)
                            {
                                if (j == sourcePos) continue;
                                var targetExplode = Object.Instantiate(gameUI.explodePrefab, gameUI.allFishRoot);
                                targetExplode.localPosition = GameObjectManager.FishRelativePosition(enemy, j);
                                gameUI.SetTimeout(() => { Object.Destroy(targetExplode.gameObject); }, 2000);
                            }
                        }, 400);
                        gameUI.GameState.Logs.Enqueue($"{sourceName}{sourcePos}号位置的鱼使用了被动技能：队友承伤。");
                        break;
                    case "reduce":
                    {
                        var shield = Object.Instantiate(gameUI.shieldEffect, gameUI.allFishRoot);
                        shield.localPosition = GameObjectManager.FishRelativePosition(enemy, sourcePos);
                        gameUI.SetTimeout(() => { Object.Destroy(shield.gameObject); }, 3000);
                        gameUI.GameState.Logs.Enqueue($"{sourceName}{sourcePos}号位置的鱼使用了被动技能：减伤。");
                        break;
                    }
                    case "heal":
                    {
                        gameUI.SetTimeout(() =>
                        {
                            var recover = Object.Instantiate(gameUI.recoverEffect, gameUI.allFishRoot);
                            recover.localPosition = GameObjectManager.FishRelativePosition(enemy, sourcePos);
                            gameUI.SetTimeout(() => { Object.Destroy(recover.gameObject); }, 2400);
                        }, 600);
                        gameUI.GameState.Logs.Enqueue($"{sourceName}{sourcePos}号位置的鱼使用了被动技能：自愈。");
                        break;
                    }
                    case "explode":
                    {
                        gameUI.SetTimeout(() =>
                        {
                            var fireBall = Object.Instantiate(gameUI.fireBallPrefab, gameUI.allFishRoot);
                            fireBall.localPosition = GameObjectManager.FishRelativePosition(enemy, sourcePos);
                            gameUI.SetTimeout(() => { Object.Destroy(fireBall.gameObject); }, 3000);
                        }, 500);
                        gameUI.GameState.Logs.Enqueue($"{sourceName}{sourcePos}号位置的鱼使用了被动技能：亡语。");
                        break;
                    }
                }
            }
        }
    }
}