using System;
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

            (myTurn ? gameUI.GameState.MyUsedTimes : gameUI.GameState.EnemyUsedTimes)[actionFish]++;

            void Subtle()
            {
                (myTurn ? gameUI.GameState.MyUsedSkills : gameUI.GameState.EnemyUsedSkills)
                    [actionFish].Add("无作为技能");

                var myselfRecover = UnityEngine.Object.Instantiate(gameUI.recoverEffect, gameUI.allFishRoot);
                myselfRecover.localPosition = GameObjectManager.FishRelativePosition(!myTurn, actionFish);
                gameUI.SetTimeout(() => { UnityEngine.Object.Destroy(myselfRecover.gameObject); }, 4000);

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

                var friendName =
                    Constants.FishName[(myTurn ? gameUI.GameState.MyFishId : gameUI.GameState.EnemyFishId)[friendId]];

                gameUI.AddLog($"{logPrefix}己方{friendName}使用了无作为技能。");

                // NEW FEATURE: Buff

                var fishId = (gameUI.GameState.MyTurn ? gameUI.GameState.MyFishId : gameUI.GameState.EnemyFishId)
                    [(int) actionInfo["ActionFish"]];
                var realFishId = fishId == 11
                    ? gameUI.GameState.MyTurn ? SharedRefs.MyImitate : SharedRefs.EnemyImitate
                    : fishId;
                var buffSet =
                    (gameUI.GameState.MyTurn ? gameUI.GameState.MyBuff : gameUI.GameState.EnemyBuff)[friendId];
                switch (realFishId)
                {
                    case 5:
                    case 7:
                        buffSet.Add(Constants.Buff.Reduce);
                        break;
                    case 6:
                        buffSet.Add(Constants.Buff.Heal);
                        break;
                    case 10:
                        buffSet.Add(Constants.Buff.Deflect);
                        break;
                }
            }

            switch ((string) actionInfo["skill"]["type"])
            {
                case "aoe":
                {
                    (myTurn ? gameUI.GameState.MyUsedSkills : gameUI.GameState.EnemyUsedSkills)
                        [actionFish].Add("AOE");
                    var targetList = actionInfo["skill"]["targets"];
                    for (var i = 0; i < targetList.Count; i++)
                    {
                        var id = (int) actionInfo["skill"]["targets"][i]["pos"];
                        gameUI.SetTimeout(() =>
                        {
                            var originalDistance =
                                GameObjectManager.FishRelativePosition(myTurn, id) -
                                GameObjectManager.FishRelativePosition(!myTurn, actionFish);
                            var targetFishId = (myTurn ? gameUI.GameState.EnemyFishId : gameUI.GameState.MyFishId)[id];
                            var bias = targetFishId == 4 ? new Vector3(4f, 0, 0) : new Vector3(3f, 0, 0);
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
                        $"{logPrefix}{(gameUI.GameState.MyTurn ? 1 : 0)}号AI发起了AOE攻击。"
                    );
                    if ((myTurn ? gameUI.GameState.MyFishSelectedAsTarget : gameUI.GameState.EnemyFishSelectedAsTarget)
                        .Any(b => b)) Subtle();
                    break;
                }
                case "infight":
                {
                    (myTurn ? gameUI.GameState.MyUsedSkills : gameUI.GameState.EnemyUsedSkills)
                        [actionFish].Add("伤害队友");
                    var target = (int) actionInfo["skill"]["targets"][0]["pos"];
                    var targetName =
                        Constants.FishName[(myTurn ? gameUI.GameState.MyFishId : gameUI.GameState.EnemyFishId)[target]];

                    var myFishExplode = UnityEngine.Object.Instantiate(gameUI.bigExplosion, gameUI.allFishRoot);
                    myFishExplode.localPosition = GameObjectManager.FishRelativePosition(!myTurn, target);
                    gameUI.SetTimeout(() => { UnityEngine.Object.Destroy(myFishExplode.gameObject); }, 2000);
                    var myFishRecover = UnityEngine.Object.Instantiate(gameUI.recoverEffect, gameUI.allFishRoot);
                    myFishRecover.localPosition = GameObjectManager.FishRelativePosition(!myTurn, actionFish);
                    gameUI.SetTimeout(() => { UnityEngine.Object.Destroy(myFishRecover.gameObject); }, 4000);
                    gameUI.AddLog($"{logPrefix}伤害了己方队友{targetName}。");
                    break;
                }
                case "crit":
                {
                    (myTurn ? gameUI.GameState.MyUsedSkills : gameUI.GameState.EnemyUsedSkills)
                        [actionFish].Add("暴击");
                    var target = (int) actionInfo["skill"]["targets"][0]["pos"];
                    var targetName =
                        Constants.FishName[(myTurn ? gameUI.GameState.EnemyFishId : gameUI.GameState.MyFishId)[target]];
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
                        $"{logPrefix}{(gameUI.GameState.MyTurn ? 1 : 0)}号AI的{targetName}发起了暴击伤害。"
                    );
                    if ((myTurn ? gameUI.GameState.MyFishSelectedAsTarget : gameUI.GameState.EnemyFishSelectedAsTarget)
                        .Any(b => b)) Subtle();
                    break;
                }
                case "subtle":
                {
                    Subtle();
                    break;
                }
            }
        }
    }
}