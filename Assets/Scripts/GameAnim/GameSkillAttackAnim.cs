using System;
using GameHelper;
using UnityEngine;
using Utils;

namespace GameAnim
{
    public static class GameSkillAttackAnim
    {
        public static void SkillAttackAnim(this GameUI gameUI)
        {
            var enemy = !gameUI.GameState.MyTurn;
            // var attackerTransforms = enemy ? _gameUI.Gom.EnemyFishTransforms : _gameUI.Gom.MyFishTransforms;
            // var attackeeTransforms = enemy ? _gameUI.Gom.MyFishTransforms : _gameUI.Gom.EnemyFishTransforms;
            var attackerSelected =
                enemy ? gameUI.GameState.EnemyFishSelectedAsTarget : gameUI.GameState.MyFishSelectedAsTarget;
            var attackeeSelected =
                enemy ? gameUI.GameState.MyFishSelectedAsTarget : gameUI.GameState.EnemyFishSelectedAsTarget;
            var attacker = enemy ? gameUI.GameState.EnemyFishSelected : gameUI.GameState.MyFishSelected;
            switch (gameUI.GameState.ActionSkill)
            {
                case Constants.Skill.Aoe:
                    for (int cnt = 0, i = 0; i < 4; i++)
                    {
                        if (!attackeeSelected[i]) continue;
                        var id = i;
                        gameUI.SetTimeout(() =>
                        {
                            var originalDistance =
                                GameObjectManager.FishRelativePosition(!enemy, id) -
                                GameObjectManager.FishRelativePosition(enemy, attacker);
                            var distance = originalDistance.x < 0
                                ? originalDistance + new Vector3(4.5f, 0, 0)
                                : originalDistance - new Vector3(4.5f, 0, 0);
                            UnityEngine.Object.Instantiate(
                                gameUI.waterProjectile,
                                GameObjectManager.FishRelativePosition(enemy, attacker) +
                                new Vector3(3, 0, 0) * (enemy ? -1 : 1),
                                Quaternion.Euler(
                                    new Vector3(0,
                                        Convert.ToInt32(Math.Atan(distance.x / distance.z) / Math.PI * 180.0),
                                        0
                                    )
                                )
                            );
                        }, cnt * 120);
                        gameUI.GameState.PassiveList.Add(i);
                        cnt++;
                    }
                    break;
                case Constants.Skill.Infight:
                    var poorFish = 0;
                    for (var i = 0; i < 4; i++)
                    {
                        // ReSharper disable once InvertIf
                        if (attackerSelected[i])
                        {
                            poorFish = i;
                            break;
                        }
                    }
                    var myFishExplode = UnityEngine.Object.Instantiate(gameUI.bigExplosion, gameUI.allFishRoot);
                    myFishExplode.localPosition = GameObjectManager.FishRelativePosition(enemy, poorFish);
                    gameUI.SetTimeout(() => { UnityEngine.Object.Destroy(myFishExplode.gameObject); }, 2000);
                    var myFishRecover = UnityEngine.Object.Instantiate(gameUI.recoverEffect, gameUI.allFishRoot);
                    myFishRecover.localPosition = GameObjectManager.FishRelativePosition(enemy, attacker);
                    gameUI.SetTimeout(() => { UnityEngine.Object.Destroy(myFishRecover.gameObject); }, 4000);
                    break;
                case Constants.Skill.CritValue:
                case Constants.Skill.CritPercent:
                    for (var i = 0; i < 4; i++)
                    {
                        // ReSharper disable once InvertIf
                        if (enemy && gameUI.GameState.MyFishSelectedAsTarget[i] ||
                            gameUI.GameState.EnemyFishSelectedAsTarget[i])
                        {
                            var target = i;
                            var distance =
                                GameObjectManager.FishRelativePosition(enemy, attacker) -
                                GameObjectManager.FishRelativePosition(!enemy, target);
                            gameUI.Repeat(cnt =>
                            {
                                (enemy ? gameUI.Gom.EnemyFishTransforms : gameUI.Gom.MyFishTransforms)[attacker]
                                    .localPosition =
                                    GameObjectManager.FishRelativePosition(!enemy, target) +
                                    Math.Abs(cnt - 20f) / 20f * distance;
                            }, () => { }, 41, 0, 10);
                            gameUI.SetTimeout(() =>
                            {
                                var targetExplode =
                                    UnityEngine.Object.Instantiate(gameUI.explodePrefab, gameUI.allFishRoot);
                                targetExplode.localPosition =
                                    GameObjectManager.FishRelativePosition(!enemy, target);
                                gameUI.SetTimeout(() => { UnityEngine.Object.Destroy(targetExplode.gameObject); },
                                    1000);
                            }, 200);
                            gameUI.GameState.PassiveList.Add(i);
                            break;
                        }
                    }
                    break;
                case Constants.Skill.Subtle:
                    var friendId = attacker;
                    for (var i = 0; i < 4; i++)
                    {
                        // ReSharper disable once InvertIf
                        if (attackeeSelected[i])
                        {
                            friendId = i;
                            break;
                        }
                    }
                    var shield = UnityEngine.Object.Instantiate(gameUI.shieldEffect, gameUI.allFishRoot);
                    shield.localPosition = GameObjectManager.FishRelativePosition(enemy, friendId);
                    gameUI.SetTimeout(() => { UnityEngine.Object.Destroy(shield.gameObject); }, 5000);

                    var myselfRecover = UnityEngine.Object.Instantiate(gameUI.recoverEffect, gameUI.allFishRoot);
                    myselfRecover.localPosition = GameObjectManager.FishRelativePosition(enemy, attacker);
                    gameUI.SetTimeout(() => { UnityEngine.Object.Destroy(myselfRecover.gameObject); }, 4000);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}