using GameHelper;
using LitJson;

namespace GameAnim
{
    public static class GamePassiveAnim
    {
        public static void PassiveAnim(this GameUI gameUI, JsonData actionInfo)
        {
            var actionFish = (int) actionInfo["ActionFish"];
            var passiveList = actionInfo["passive"];
            for (var i = 0; i < passiveList.Count; i++)
            {
                var sourcePos = (int) passiveList[i]["source"];
                var enemy = (bool) passiveList[i]["isEnemy"];
                switch ((string) passiveList[i]["type"])
                {
                    case "counter":
                        break;
                    case "deflect":
                        break;
                    case "reduce":
                    {
                        var shield = UnityEngine.Object.Instantiate(gameUI.shieldEffect, gameUI.allFishRoot);
                        shield.localPosition = GameObjectManager.FishRelativePosition(enemy, sourcePos);
                        gameUI.SetTimeout(() => { UnityEngine.Object.Destroy(shield.gameObject); }, 5000);
                        break;
                    }
                    case "heal":
                    {
                        var recover = UnityEngine.Object.Instantiate(gameUI.recoverEffect, gameUI.allFishRoot);
                        recover.localPosition = GameObjectManager.FishRelativePosition(enemy, sourcePos);
                        gameUI.SetTimeout(() => { UnityEngine.Object.Destroy(recover.gameObject); }, 4000);
                        break;
                    }
                    case "explode":
                        break;
                }
            }
        }
    }
}