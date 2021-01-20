using System;
using System.Collections.Generic;
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

        public readonly List<SkinnedMeshRenderer[]> MyFishMeshRenderers = new List<SkinnedMeshRenderer[]>();
        public readonly List<SkinnedMeshRenderer[]> EnemyFishMeshRenderers = new List<SkinnedMeshRenderer[]>();

        public readonly List<ParticleSystem> MyFishParticleSystems = new List<ParticleSystem>();
        public readonly List<ParticleSystem> EnemyFishParticleSystems = new List<ParticleSystem>();

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
                        if (enemy)
                            _gameStates.EnemyFishSelectedAsTarget[j] = !_gameStates.EnemyFishSelectedAsTarget[j];
                        else
                            _gameStates.MyFishSelectedAsTarget[j] = !_gameStates.MyFishSelectedAsTarget[j];
                        break;
                    case Constants.GameStatus.WaitingAnimation:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });
            fishTransform.GetComponent<EventTrigger>().triggers.Add(fishTrigger);
            return fishTransform;
        }

        public void Init(GameUI gameUI)
        {
            for (var i = 0; i < 4; i++)
            {
                var myFish = GenFish(false, i, gameUI);
                MyFishTransforms.Add(myFish);
                MyFishMeshRenderers.Add(myFish.GetComponentsInChildren<SkinnedMeshRenderer>());
                MyFishParticleSystems.Add(myFish.GetComponentInChildren<ParticleSystem>());

                var enemyFish = GenFish(true, i, gameUI);
                EnemyFishTransforms.Add(enemyFish);
                EnemyFishMeshRenderers.Add(enemyFish.GetComponentsInChildren<SkinnedMeshRenderer>());
                EnemyFishParticleSystems.Add(enemyFish.GetComponentInChildren<ParticleSystem>());

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
                gameUI.enemyProfiles[i].SetAtk(Constants.DefaultAtk);
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
                    MyFishMeshRenderers[i] = MyFishTransforms[i].GetComponentsInChildren<SkinnedMeshRenderer>();
                    MyFishParticleSystems[i] = MyFishTransforms[i].GetComponentInChildren<ParticleSystem>();
                    MyFogs[i].gameObject.SetActive(!(bool) lastPlayers[0]["fight_fish"][i]["is_expose"]);
                }
                // ReSharper disable once InvertIf
                if ((float) players[1]["fight_fish"][i]["hp"] <= 0 &&
                    (float) lastPlayers[1]["fight_fish"][i]["hp"] > 0)
                {
                    EnemyFishTransforms[i] = GenFish(true, i, gameUI);
                    EnemyFishMeshRenderers[i] = EnemyFishTransforms[i].GetComponentsInChildren<SkinnedMeshRenderer>();
                    EnemyFishParticleSystems[i] = EnemyFishTransforms[i].GetComponentInChildren<ParticleSystem>();
                    EnemyFogs[i].gameObject.SetActive(!(bool) lastPlayers[1]["fight_fish"][i]["is_expose"]);
                }
            }
        }

        public GameObjectManager(GameStates gameStates)
        {
            _gameStates = gameStates;
        }
    }
}