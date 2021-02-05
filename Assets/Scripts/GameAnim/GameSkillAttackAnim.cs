using System;
using System.Collections.Generic;
using System.Linq;
using GameHelper;
using LitJson;
using UnityEngine;
using Utils;

namespace GameAnim
{
    public static class GameSkillAttackAnim
    {
        public static void SkillAttackAnim(this GameUI gameUI, JsonData actionInfo, string logPrefix)
        {
            var myTurn = gameUI.GameState.MyTurn;
            var actionFish = (int) actionInfo["ActionFish"];

            void Subtle()
            {
                if (!myTurn) gameUI.GameState.EnemyUsedSkills[actionFish].Add("无作为技能");

                var myselfRecover = UnityEngine.Object.Instantiate(gameUI.recoverEffect, gameUI.allFishRoot);
                myselfRecover.localPosition = GameObjectManager.FishRelativePosition(!myTurn, actionFish);
                gameUI.SetTimeout(() => { UnityEngine.Object.Destroy(myselfRecover.gameObject); }, 4000);

                if (SharedRefs.Mode == Constants.GameMode.Online && !myTurn)
                {
                    gameUI.AddLog($"{logPrefix}己方使用了无作为技能。");
                    return;
                }

                var friendId = actionFish;
                for (var i = 0; i < 4; i++)
                {
                    if ((!myTurn || !gameUI.GameState.MyFishSelectedAsTarget[i]) &&
                        (myTurn || !gameUI.GameState.EnemyFishSelectedAsTarget[i])) continue;
                    friendId = i;
                    break;
                }

                var shield = UnityEngine.Object.Instantiate(gameUI.shieldEffect, gameUI.allFishRoot);
                shield.localPosition = GameObjectManager.FishRelativePosition(!myTurn, friendId);
                gameUI.SetTimeout(() => { UnityEngine.Object.Destroy(shield.gameObject); }, 5000);

                gameUI.AddLog($"{logPrefix}己方{friendId}号位置的鱼使用了无作为技能。");
            }

            switch ((string) actionInfo["skill"]["type"])
            {
                case "aoe":
                {
                    if (!myTurn) gameUI.GameState.EnemyUsedSkills[actionFish].Add("AOE");
                    var targetList = actionInfo["skill"]["targets"];
                    var ids = new List<string>();
                    for (var i = 0; i < targetList.Count; i++)
                    {
                        var id = (int) actionInfo["skill"]["targets"][i]["pos"];
                        ids.Add(id.ToString());
                        gameUI.SetTimeout(() =>
                        {
                            var originalDistance =
                                GameObjectManager.FishRelativePosition(myTurn, id) -
                                GameObjectManager.FishRelativePosition(!myTurn, actionFish);
                            var targetFishId = (myTurn ? gameUI.GameState.EnemyFishId : gameUI.GameState.MyFishId)[id];
                            var bias = targetFishId == 4 ? new Vector3(4.5f, 0, 0) : new Vector3(3f, 0, 0);
                            var distance = originalDistance + (originalDistance.x < 0 ? 1 : -1) * bias;
                            var angle = Math.Atan(distance.x / distance.z) / Math.PI * 180.0;
                            UnityEngine.Object.Instantiate(
                                gameUI.waterProjectile,
                                GameObjectManager.FishRelativePosition(!myTurn, actionFish) +
                                new Vector3(3, 0, 0) * (myTurn ? -1 : 1),
                                Quaternion.Euler(
                                    new Vector3(
                                        0,
                                        Convert.ToInt32(myTurn
                                            ? angle < 0 ? angle : angle - 180.0
                                            : angle > 0
                                                ? angle
                                                : angle + 180.0),
                                        0
                                    )
                                )
                            );
                        }, i * 120);
                    }
                    gameUI.AddLog(
                        $"{logPrefix}{(gameUI.GameState.MyTurn ? GameUI.EnemyStr : GameUI.MeStr)}{string.Join(",", ids)}号位置的鱼发起了AOE攻击。"
                    );
                    if (SharedRefs.Mode == Constants.GameMode.Offline &&
                        (myTurn ? gameUI.GameState.MyFishSelectedAsTarget : gameUI.GameState.EnemyFishSelectedAsTarget)
                        .Any()) Subtle();
                    break;
                }
                case "infight":
                {
                    if (!myTurn) gameUI.GameState.EnemyUsedSkills[actionFish].Add("伤害队友");
                    var target = (int) actionInfo["skill"]["targets"][0]["pos"];
                    var myFishExplode = UnityEngine.Object.Instantiate(gameUI.bigExplosion, gameUI.allFishRoot);
                    myFishExplode.localPosition = GameObjectManager.FishRelativePosition(!myTurn, target);
                    gameUI.SetTimeout(() => { UnityEngine.Object.Destroy(myFishExplode.gameObject); }, 2000);
                    var myFishRecover = UnityEngine.Object.Instantiate(gameUI.recoverEffect, gameUI.allFishRoot);
                    myFishRecover.localPosition = GameObjectManager.FishRelativePosition(!myTurn, actionFish);
                    gameUI.SetTimeout(() => { UnityEngine.Object.Destroy(myFishRecover.gameObject); }, 4000);
                    gameUI.AddLog($"{logPrefix}伤害了己方{target}号位置队友。");
                    break;
                }
                case "crit":
                {
                    if (!myTurn) gameUI.GameState.EnemyUsedSkills[actionFish].Add("暴击");
                    var target = (int) actionInfo["skill"]["targets"][0]["pos"];
                    var distance =
                        GameObjectManager.FishRelativePosition(!myTurn, actionFish) -
                        GameObjectManager.FishRelativePosition(myTurn, target);
                    gameUI.Repeat(cnt =>
                    {
                        (myTurn ? gameUI.Gom.MyFishTransforms : gameUI.Gom.EnemyFishTransforms)
                            [actionFish].localPosition =
                            GameObjectManager.FishRelativePosition(myTurn, target) +
                            Math.Abs(cnt - 5f) / 5f * distance;
                    }, () => { }, 11, 0, 40);
                    gameUI.SetTimeout(() =>
                    {
                        var targetExplode =
                            UnityEngine.Object.Instantiate(gameUI.explodePrefab, gameUI.allFishRoot);
                        targetExplode.localPosition = GameObjectManager.FishRelativePosition(myTurn, target);
                        gameUI.SetTimeout(() => { UnityEngine.Object.Destroy(targetExplode.gameObject); }, 1000);
                    }, 200);
                    gameUI.AddLog(
                        $"{logPrefix}{(gameUI.GameState.MyTurn ? GameUI.EnemyStr : GameUI.MeStr)}{target}号位置的鱼发起了暴击伤害。"
                    );
                    if (SharedRefs.Mode == Constants.GameMode.Offline &&
                        (myTurn ? gameUI.GameState.MyFishSelectedAsTarget : gameUI.GameState.EnemyFishSelectedAsTarget)
                        .Any()) Subtle();
                    break;
                }
                case "subtle":
                {
                    Subtle();
                    break;
                }
            }
            if (SharedRefs.Mode == Constants.GameMode.Online &&
                !myTurn && !gameUI.GameState.EnemyFishExpose[actionFish])
                GameObjectManager.UpdateHiddenExtension(gameUI, actionFish);
        }
    }
}