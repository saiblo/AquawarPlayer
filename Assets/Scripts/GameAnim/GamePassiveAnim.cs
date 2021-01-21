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
                var sourceName = enemy ? GameUI.EnemyStr : GameUI.MeStr;
                switch ((string) passiveList[i]["type"])
                {
                    case "counter":
                    {
                        if (enemy) gameUI.GameState.EnemyUsedPassives[sourcePos].Add("膨胀反伤");
                        gameUI.SetTimeout(() =>
                        {
                            var explosion = Object.Instantiate(gameUI.smallExplosion, gameUI.allFishRoot);
                            explosion.localPosition = GameObjectManager.FishRelativePosition(enemy, sourcePos);
                            gameUI.SetTimeout(() => { Object.Destroy(explosion.gameObject); }, 1800);
                        }, 500);
                        gameUI.AddLog($"{sourceName}{sourcePos}号位置的鱼使用了被动技能：膨胀反伤。");
                        break;
                    }
                    case "deflect":
                        if (enemy) gameUI.GameState.EnemyUsedPassives[sourcePos].Add("队友承伤");
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
                        gameUI.AddLog($"{sourceName}{sourcePos}号位置的鱼使用了被动技能：队友承伤。");
                        break;
                    case "reduce":
                    {
                        if (enemy) gameUI.GameState.EnemyUsedPassives[sourcePos].Add("减伤");
                        var shield = Object.Instantiate(gameUI.shieldEffect, gameUI.allFishRoot);
                        shield.localPosition = GameObjectManager.FishRelativePosition(enemy, sourcePos);
                        gameUI.SetTimeout(() => { Object.Destroy(shield.gameObject); }, 3000);
                        gameUI.AddLog($"{sourceName}{sourcePos}号位置的鱼使用了被动技能：减伤。");
                        break;
                    }
                    case "heal":
                    {
                        if (enemy) gameUI.GameState.EnemyUsedPassives[sourcePos].Add("自愈");
                        gameUI.SetTimeout(() =>
                        {
                            var recover = Object.Instantiate(gameUI.recoverEffect, gameUI.allFishRoot);
                            recover.localPosition = GameObjectManager.FishRelativePosition(enemy, sourcePos);
                            gameUI.SetTimeout(() => { Object.Destroy(recover.gameObject); }, 2400);
                        }, 600);
                        gameUI.AddLog($"{sourceName}{sourcePos}号位置的鱼使用了被动技能：自愈。");
                        break;
                    }
                    case "explode":
                    {
                        if (enemy) gameUI.GameState.EnemyUsedPassives[sourcePos].Add("亡语");
                        gameUI.SetTimeout(() =>
                        {
                            var fireBall = Object.Instantiate(gameUI.fireBallPrefab, gameUI.allFishRoot);
                            fireBall.localPosition = GameObjectManager.FishRelativePosition(enemy, sourcePos);
                            gameUI.SetTimeout(() => { Object.Destroy(fireBall.gameObject); }, 3000);
                        }, 500);
                        gameUI.AddLog($"{sourceName}{sourcePos}号位置的鱼使用了被动技能：亡语。");
                        break;
                    }
                }
                if (enemy && !gameUI.GameState.EnemyFishExpose[sourcePos])
                    GameObjectManager.UpdateHiddenExtension(gameUI, sourcePos);
            }
        }
    }
}