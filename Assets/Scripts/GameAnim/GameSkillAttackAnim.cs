using System;
using GameHelper;
using LitJson;
using UnityEngine;

namespace GameAnim
{
    public static class GameSkillAttackAnim
    {
        public static void SkillAttackAnim(this GameUI gameUI, JsonData actionInfo)
        {
            var myTurn = gameUI.GameState.MyTurn;
            var actionFish = (int) actionInfo["ActionFish"];
            switch ((string) actionInfo["skill"]["type"])
            {
                case "aoe":
                {
                    var targetList = actionInfo["skill"]["targets"];
                    for (var i = 0; i < targetList.Count; i++)
                    {
                        var id = (int) actionInfo["skill"]["targets"][i]["pos"];
                        gameUI.SetTimeout(() =>
                        {
                            var originalDistance =
                                GameObjectManager.FishRelativePosition(myTurn, id) -
                                GameObjectManager.FishRelativePosition(!myTurn, actionFish);
                            var distance = originalDistance.x < 0
                                ? originalDistance + new Vector3(4.5f, 0, 0)
                                : originalDistance - new Vector3(4.5f, 0, 0);
                            var angle = Math.Atan(distance.x / distance.z) / Math.PI * 180.0;
                            UnityEngine.Object.Instantiate(
                                gameUI.waterProjectile,
                                GameObjectManager.FishRelativePosition(!myTurn, actionFish) +
                                new Vector3(3, 0, 0) * (myTurn ? -1 : 1),
                                Quaternion.Euler(
                                    new Vector3(
                                        0,
                                        Convert.ToInt32(angle < 0 ? angle : angle + 180.0),
                                        0
                                    )
                                )
                            );
                        }, i * 120);
                    }
                    break;
                }
                case "infight":
                {
                    var target = (int) actionInfo["skill"]["targets"][0]["pos"];
                    var myFishExplode = UnityEngine.Object.Instantiate(gameUI.bigExplosion, gameUI.allFishRoot);
                    myFishExplode.localPosition = GameObjectManager.FishRelativePosition(!myTurn, target);
                    gameUI.SetTimeout(() => { UnityEngine.Object.Destroy(myFishExplode.gameObject); }, 2000);
                    var myFishRecover = UnityEngine.Object.Instantiate(gameUI.recoverEffect, gameUI.allFishRoot);
                    myFishRecover.localPosition = GameObjectManager.FishRelativePosition(!myTurn, actionFish);
                    gameUI.SetTimeout(() => { UnityEngine.Object.Destroy(myFishRecover.gameObject); }, 4000);
                    break;
                }
                case "crit":
                {
                    var target = (int) actionInfo["skill"]["targets"][0]["pos"];
                    var distance =
                        GameObjectManager.FishRelativePosition(!myTurn, actionFish) -
                        GameObjectManager.FishRelativePosition(myTurn, target);
                    gameUI.Repeat(cnt =>
                    {
                        (myTurn ? gameUI.Gom.MyFishTransforms : gameUI.Gom.EnemyFishTransforms)
                            [actionFish].localPosition =
                            GameObjectManager.FishRelativePosition(myTurn, target) +
                            Math.Abs(cnt - 20f) / 20f * distance;
                    }, () => { }, 41, 0, 10);
                    gameUI.SetTimeout(() =>
                    {
                        var targetExplode =
                            UnityEngine.Object.Instantiate(gameUI.explodePrefab, gameUI.allFishRoot);
                        targetExplode.localPosition = GameObjectManager.FishRelativePosition(myTurn, target);
                        gameUI.SetTimeout(() => { UnityEngine.Object.Destroy(targetExplode.gameObject); }, 1000);
                    }, 200);
                    break;
                }
                case "subtle":
                {
                    if (!myTurn) break;
                    var friendId = actionFish;
                    for (var i = 0; i < 4; i++)
                    {
                        if (!gameUI.GameState.MyFishSelectedAsTarget[i]) continue;
                        friendId = i;
                        break;
                    }

                    var shield = UnityEngine.Object.Instantiate(gameUI.shieldEffect, gameUI.allFishRoot);
                    shield.localPosition = GameObjectManager.FishRelativePosition(false, friendId);
                    gameUI.SetTimeout(() => { UnityEngine.Object.Destroy(shield.gameObject); }, 5000);

                    var myselfRecover = UnityEngine.Object.Instantiate(gameUI.recoverEffect, gameUI.allFishRoot);
                    myselfRecover.localPosition = GameObjectManager.FishRelativePosition(false, actionFish);
                    gameUI.SetTimeout(() => { UnityEngine.Object.Destroy(myselfRecover.gameObject); }, 4000);
                    break;
                }
            }
        }
    }
}