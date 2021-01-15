using System;
using System.Linq;
using GameHelper;
using LitJson;

namespace GameAnim
{
    public static class GameNormalAttackAnim
    {
        public static void NormalAttackAnim(this GameUI gameUI, JsonData actionInfo)
        {
            var actionFish = (int) actionInfo["ActionFish"];
            var target = (int) actionInfo["hit"].OfType<JsonData>().First(e => (bool) e["traceable"])["target"];

            var myTurn = gameUI.GameState.MyTurn;
            var distance =
                GameObjectManager.FishRelativePosition(!myTurn, actionFish) -
                GameObjectManager.FishRelativePosition(myTurn, target);
            gameUI.Repeat(cnt =>
            {
                (myTurn ? gameUI.Gom.MyFishTransforms : gameUI.Gom.EnemyFishTransforms)[actionFish].localPosition =
                    GameObjectManager.FishRelativePosition(myTurn, target) +
                    Math.Abs(cnt - 40f) / 40f * distance;
            }, () => { }, 81, 0, 10);
        }
    }
}