using System;
using System.Linq;
using GameHelper;
using LitJson;
using UnityEngine;
using Utils;

namespace GameAnim
{
    public static class GameNormalAttackAnim
    {
        public static void NormalAttackAnim(this GameUI gameUI, JsonData actionInfo, string logPrefix)
        {
            var actionFish = (int) actionInfo["ActionFish"];
            var target = (int) actionInfo["hit"][0]["target"];
            try
            {
                target = (int) actionInfo["hit"].OfType<JsonData>().First(e => (bool) e["traceable"])["target"];
            }
            catch (Exception e)
            {
                Debug.Log($"@{SharedRefs.ReplayCursor}");
                Debug.Log(e);
            }

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

            var fishId = (gameUI.GameState.MyTurn ? gameUI.GameState.EnemyFishId : gameUI.GameState.MyFishId)[target];
            var fishName = Constants.FishName[fishId];
            var side = gameUI.GameState.MyTurn ? GameUI.EnemyStr : GameUI.MeStr;
            gameUI.AddLog($"{logPrefix}{side}{target}号位置的{fishName}发起了普通攻击。");
        }
    }
}