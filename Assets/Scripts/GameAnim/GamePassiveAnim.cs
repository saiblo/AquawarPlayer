using GameHelper;
using LitJson;
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
                switch ((string) passiveList[i]["type"])
                {
                    case "counter":
                    {
                        gameUI.SetTimeout(() =>
                        {
                            var explosion = UnityEngine.Object.Instantiate(gameUI.smallExplosion, gameUI.allFishRoot);
                            explosion.localPosition = GameObjectManager.FishRelativePosition(enemy, sourcePos);
                            gameUI.SetTimeout(() => { UnityEngine.Object.Destroy(explosion.gameObject); }, 1800);
                        }, 500);
                        break;
                    }
                    case "deflect":
                        break;
                    case "reduce":
                    {
                        var shield = UnityEngine.Object.Instantiate(gameUI.shieldEffect, gameUI.allFishRoot);
                        shield.localPosition = GameObjectManager.FishRelativePosition(enemy, sourcePos);
                        gameUI.SetTimeout(() => { UnityEngine.Object.Destroy(shield.gameObject); }, 3000);
                        break;
                    }
                    case "heal":
                    {
                        gameUI.SetTimeout(() =>
                        {
                            var recover = UnityEngine.Object.Instantiate(gameUI.recoverEffect, gameUI.allFishRoot);
                            recover.localPosition = GameObjectManager.FishRelativePosition(enemy, sourcePos);
                            gameUI.SetTimeout(() => { UnityEngine.Object.Destroy(recover.gameObject); }, 2400);
                        }, 600);
                        break;
                    }
                    case "explode":
                    {
                        gameUI.SetTimeout(() =>
                        {
                            var fireBall = UnityEngine.Object.Instantiate(gameUI.fireBallPrefab, gameUI.allFishRoot);
                            fireBall.localPosition = GameObjectManager.FishRelativePosition(enemy, sourcePos);
                            gameUI.SetTimeout(() => { UnityEngine.Object.Destroy(fireBall.gameObject); }, 3000);
                        }, 500);
                        break;
                    }
                }
            }
        }
    }
}