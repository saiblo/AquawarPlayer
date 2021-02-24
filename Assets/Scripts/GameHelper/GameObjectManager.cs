using System;
using System.Collections.Generic;
using Components;
using UnityEngine;
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

        public GameObject GuessFish = null;

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

        private Transform GenFish(bool enemy, int j, GameBridge gameUI)
        {
            var fishTransform = Object.Instantiate(
                SharedRefs.FishPrefabs[(enemy ? _gameStates.EnemyFishId : _gameStates.MyFishId)[j]],
                gameUI.allFishRoot
            );
            fishTransform.localPosition = FishRelativePosition(enemy, j);
            fishTransform.localScale = Small;
            fishTransform.rotation = Quaternion.Euler(new Vector3(0, enemy ? 100 : 260, 0));
            return fishTransform;
        }

        public void Init(GameUI gameUI)
        {
            for (var i = 0; i < 4; i++)
            {
                MyFishTransforms.Add(GenFish(false, i, gameUI));
                EnemyFishTransforms.Add(GenFish(true, i, gameUI));

                var myFishId = gameUI.GameState.MyFishId[i];
                var myFishRealId = myFishId == 11 ? SharedRefs.MyImitate : myFishId;
                var enemyFishId = gameUI.GameState.EnemyFishId[i];
                var enemyFishRealId = enemyFishId == 11 ? SharedRefs.EnemyImitate : enemyFishId;
                gameUI.myProfiles[i].SetupFish(
                    myFishId,
                    gameUI.myExtensions[i],
                    myFishId == 11 ? SharedRefs.MyImitate : -1
                );
                gameUI.myExtensions[i].UpdateText(
                    $"{Constants.FishName[myFishId]}\n主动：{Constants.SkillDescription[myFishRealId]}\n被动：{Constants.PassiveDescription[myFishRealId]}"
                );
                gameUI.enemyProfiles[i].SetupFish(
                    enemyFishId,
                    gameUI.enemyExtensions[i],
                    enemyFishId == 11 ? SharedRefs.EnemyImitate : -1
                );
                gameUI.enemyExtensions[i].UpdateText(
                    $"{Constants.FishName[enemyFishId]}\n主动：{Constants.SkillDescription[enemyFishRealId]}\n被动：{Constants.PassiveDescription[enemyFishRealId]}"
                );

                gameUI.myStatus[i].Full = Constants.DefaultHp;
                gameUI.enemyStatus[i].Full = Constants.DefaultHp;
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
                if ((float) players[0]["fight_fish"][i]["hp"] <= 0 && (float) lastPlayers[0]["fight_fish"][i]["hp"] > 0)
                    MyFishTransforms[i] = GenFish(false, i, gameUI);
                MyFogs[i].gameObject.SetActive(gameUI.GameState.MyFishAlive[i] &&
                                               !(bool) lastPlayers[0]["fight_fish"][i]["is_expose"]);
                if ((float) players[1]["fight_fish"][i]["hp"] <= 0 && (float) lastPlayers[1]["fight_fish"][i]["hp"] > 0)
                    EnemyFishTransforms[i] = GenFish(true, i, gameUI);
                EnemyFogs[i].gameObject.SetActive(gameUI.GameState.EnemyFishAlive[i] &&
                                                  !(bool) lastPlayers[1]["fight_fish"][i]["is_expose"]);
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