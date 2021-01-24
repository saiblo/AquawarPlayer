using System;
using System.Collections.Generic;
using System.Linq;
using Components;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;
using Object = UnityEngine.Object;

namespace GameHelper
{
    /// <summary>
    ///   <para>Manages the references and initializations of the game objects.</para>
    /// </summary>
    public class GameObjectManager
    {
        // Holds some necessary references
        private readonly GameStates _gameStates;

        // The fog
        public readonly List<Transform> MyFogs = new List<Transform>();
        public readonly List<Transform> EnemyFogs = new List<Transform>();

        // Fish-object related
        public readonly List<Transform> MyFishTransforms = new List<Transform>();
        public readonly List<Transform> EnemyFishTransforms = new List<Transform>();

        // Misc
        public readonly Vector3 Small = new Vector3(3, 3, 3);
        public readonly Vector3 Large = new Vector3(4, 4, 4);

        private CountDown _countDown;

        public void StopCountDown(GameUI gameUI)
        {
            try
            {
                if (_countDown) Object.Destroy(_countDown.gameObject);
            }
            catch (Exception)
            {
                // ignored
            }
            _countDown = null;
            gameUI.roundText.text = "倒计时：--";
        }

        public void ResetCountDown(GameUI gameUI)
        {
            try
            {
                if (_countDown) Object.Destroy(_countDown.gameObject);
            }
            catch (Exception)
            {
                // ignored
            }
            _countDown = Object.Instantiate(gameUI.countDownPrefab);
            _countDown.StartTiming(gameUI.roundText);
        }

        public static Vector3 FishRelativePosition(bool enemy, int id)
        {
            return new Vector3(
                (enemy ? -1 : 1) * 7 - 1,
                0,
                5 - id * 3
            );
        }

        // Helper functions

        public bool Initialized;

        public Transform GenFish(bool enemy, int j, GameUI gameUI)
        {
            var fishTransform = Object.Instantiate(
                SharedRefs.Mode == Constants.GameMode.Online && enemy && !_gameStates.EnemyFishExpose[j]
                    ? gameUI.unkFishPrefab
                    : SharedRefs.FishPrefabs[(enemy ? _gameStates.EnemyFishId : _gameStates.MyFishId)[j]],
                gameUI.allFishRoot);
            fishTransform.localPosition = FishRelativePosition(enemy, j);
            fishTransform.localScale = Small;
            fishTransform.rotation = Quaternion.Euler(new Vector3(0, enemy ? 100 : 260, 0));
            if (SharedRefs.Mode == Constants.GameMode.Offline) return fishTransform;

            if (!enemy) gameUI.actionButtons[j].Setup(gameUI.GameState.MyFishId[j]);

            var fishTrigger = new EventTrigger.Entry();
            fishTrigger.callback.AddListener(delegate
            {
                switch (_gameStates.GameStatus)
                {
                    case Constants.GameStatus.DoAssertion:
                        if (enemy)
                        {
                            if (gameUI.GameState.EnemyFishExpose[j]) break;
                            _gameStates.Assertion = _gameStates.Assertion == j ? -1 : j;
                            gameUI.CloseAssertionModal();
                        }
                        break;
                    case Constants.GameStatus.WaitAssertion:
                        break;
                    case Constants.GameStatus.SelectMyFish:
                        if (!enemy) _gameStates.MyFishSelected = _gameStates.MyFishSelected == j ? -1 : j;
                        break;
                    case Constants.GameStatus.SelectEnemyFish:
                        if (_gameStates.NormalAttack)
                        {
                            if (!enemy) break;
                            if (_gameStates.EnemyFishSelectedAsTarget[j])
                                _gameStates.EnemyFishSelectedAsTarget[j] = false;
                            else
                            {
                                for (var i = 0; i < 4; i++)
                                    _gameStates.EnemyFishSelectedAsTarget[i] = false;
                                _gameStates.EnemyFishSelectedAsTarget[j] = true;
                            }
                        }
                        else
                        {
                            var fishId = gameUI.GameState.MyFishId[gameUI.GameState.MyFishSelected];
                            var fishSkill = Constants.SkillDict[fishId == 11 ? SharedRefs.MyImitate : fishId];
                            if (enemy)
                            {
                                if (fishSkill == Constants.Skill.Aoe ||
                                    fishSkill == Constants.Skill.InFight ||
                                    fishSkill == Constants.Skill.InHelp ||
                                    fishSkill == Constants.Skill.Clown) break;
                                if (fishSkill == Constants.Skill.MinCrit)
                                {
                                    var minVal =
                                        (from hp in gameUI.enemyStatus
                                            where hp.Current > 0
                                            select hp.Current).Min();
                                    if (gameUI.enemyStatus[j].Current != minVal) break;
                                }
                                if (fishSkill == Constants.Skill.Turtle &&
                                    (fishId == 11 ? gameUI.GameState.ImitateUsed : gameUI.GameState.TurtleUsed) >= 3)
                                    break;
                                if (_gameStates.EnemyFishSelectedAsTarget[j])
                                    _gameStates.EnemyFishSelectedAsTarget[j] = false;
                                else
                                {
                                    for (var i = 0; i < 4; i++) _gameStates.EnemyFishSelectedAsTarget[i] = false;
                                    _gameStates.EnemyFishSelectedAsTarget[j] = true;
                                }
                            }
                            else
                            {
                                if (fishSkill == Constants.Skill.Aoe ||
                                    fishSkill == Constants.Skill.Crit ||
                                    fishSkill == Constants.Skill.MinCrit) break;
                                if ((fishSkill == Constants.Skill.InFight ||
                                     fishSkill == Constants.Skill.Turtle ||
                                     fishSkill == Constants.Skill.Clown) &&
                                    j == gameUI.GameState.MyFishSelected) break;
                                if (_gameStates.MyFishSelectedAsTarget[j])
                                    _gameStates.MyFishSelectedAsTarget[j] = false;
                                else
                                {
                                    for (var i = 0; i < 4; i++) _gameStates.MyFishSelectedAsTarget[i] = false;
                                    _gameStates.MyFishSelectedAsTarget[j] = true;
                                }
                            }
                        }
                        break;
                    case Constants.GameStatus.WaitingAnimation:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });
            fishTransform.GetComponent<EventTrigger>().triggers.Add(fishTrigger);
            if (enemy) return fishTransform;

            gameUI.counters[j].gameObject.SetActive(
                gameUI.GameState.MyFishId[j] == 6 || gameUI.GameState.MyFishId[j] == 10 ||
                gameUI.GameState.MyFishId[j] == 11 && (SharedRefs.MyImitate == 6 || SharedRefs.MyImitate == 10)
            );
            return fishTransform;
        }

        public void Init(GameUI gameUI)
        {
            for (var i = 0; i < 4; i++)
            {
                MyFishTransforms.Add(GenFish(false, i, gameUI));
                EnemyFishTransforms.Add(GenFish(true, i, gameUI));

                var myFishId = gameUI.GameState.MyFishId[i];
                var enemyFishId = gameUI.GameState.EnemyFishId[i];
                var myImitatePrompt = gameUI.GameState.MyFishId[i] == 11
                    ? $"\n所拟态鱼：{Constants.FishName[SharedRefs.MyImitate]}"
                    : "";
                gameUI.myProfiles[i].SetupFish(myFishId, gameUI.myExtensions[i]);
                gameUI.myExtensions[i].UpdateText(
                    $"{Constants.FishName[myFishId]}\n主动：{Constants.SkillDescription[myFishId]}\n被动：{Constants.PassiveDescription[myFishId]}{myImitatePrompt}"
                );
                if (SharedRefs.Mode == Constants.GameMode.Offline || _gameStates.EnemyFishExpose[i])
                {
                    var enemyImitatePrompt = gameUI.GameState.EnemyFishId[i] == 11
                        ? $"\n所拟态鱼：{Constants.FishName[SharedRefs.EnemyImitate]}"
                        : "";
                    gameUI.enemyProfiles[i].SetupFish(enemyFishId, gameUI.enemyExtensions[i]);
                    gameUI.enemyExtensions[i].UpdateText(
                        $"{Constants.FishName[enemyFishId]}\n主动：{Constants.SkillDescription[enemyFishId]}\n被动：{Constants.PassiveDescription[enemyFishId]}{enemyImitatePrompt}"
                    );
                }
                else
                {
                    gameUI.enemyProfiles[i].SetupFish(-1, gameUI.enemyExtensions[i]);
                    gameUI.enemyExtensions[i].UpdateText("隐藏");
                }

                gameUI.myStatus[i].Full = Constants.DefaultHp;
                gameUI.enemyStatus[i].Full = Constants.DefaultHp;
                gameUI.myProfiles[i].SetHp(gameUI.myStatus[i].Full);
                gameUI.enemyProfiles[i].SetHp(gameUI.enemyStatus[i].Full);
                gameUI.myProfiles[i].SetAtk(Constants.DefaultAtk);
                gameUI.enemyProfiles[i].SetAtk(-1);
            }

            for (var i = 0; i < Constants.FishNum; i++)
            {
                gameUI.myGlance.SetupFish(i,
                    gameUI.GameState.MyFishPicked.Contains(i)
                        ? Constants.FishState.Using
                        : gameUI.GameState.MyFishAvailable.Contains(i)
                            ? Constants.FishState.Free
                            : Constants.FishState.Used,
                    gameUI.myGlanceExt
                );
                gameUI.enemyGlance.SetupFish(i,
                    gameUI.GameState.EnemyFishPicked.Contains(i)
                        ? Constants.FishState.Using
                        : gameUI.GameState.EnemyFishAvailable.Contains(i)
                            ? Constants.FishState.Free
                            : Constants.FishState.Used,
                    gameUI.enemyGlanceExt
                );
            }

            Initialized = true;
        }

        public void CheckReviveOnBackwards(GameUI gameUI)
        {
            var players = SharedRefs.ReplayJson[SharedRefs.ReplayCursor]["players"];
            var lastPlayers = SharedRefs.ReplayJson[SharedRefs.ReplayCursor - 2]["players"];
            for (var i = 0; i < 4; i++)
            {
                if ((float) players[0]["fight_fish"][i]["hp"] <= 0 &&
                    (float) lastPlayers[0]["fight_fish"][i]["hp"] > 0)
                {
                    MyFishTransforms[i] = GenFish(false, i, gameUI);
                    MyFogs[i].gameObject.SetActive(!(bool) lastPlayers[0]["fight_fish"][i]["is_expose"]);
                }
                // ReSharper disable once InvertIf
                if ((float) players[1]["fight_fish"][i]["hp"] <= 0 &&
                    (float) lastPlayers[1]["fight_fish"][i]["hp"] > 0)
                {
                    EnemyFishTransforms[i] = GenFish(true, i, gameUI);
                    EnemyFogs[i].gameObject.SetActive(!(bool) lastPlayers[1]["fight_fish"][i]["is_expose"]);
                }
            }
        }

        public static void UpdateHiddenExtension(GameUI gameUI, int id)
        {
            gameUI.enemyExtensions[id].text.text =
                $"隐藏\n用过的主动：{string.Join(",", gameUI.GameState.EnemyUsedSkills[id])}\n用过的被动：{string.Join(",", gameUI.GameState.EnemyUsedPassives[id])}";
        }

        public GameObjectManager(GameStates gameStates)
        {
            _gameStates = gameStates;
        }
    }
}