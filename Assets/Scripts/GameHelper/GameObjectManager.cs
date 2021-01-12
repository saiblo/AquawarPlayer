using System;
using System.Collections.Generic;
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

        public Transform GenFish(bool enemy, int j, Transform unkFishPrefab, Transform allFishRoot)
        {
            var fishTransform = Object.Instantiate(
                SharedRefs.Mode == Constants.GameMode.Online && enemy && !_gameStates.EnemyFishExpose[j]
                    ? unkFishPrefab
                    : SharedRefs.FishPrefabs[(enemy ? _gameStates.EnemyFishId : _gameStates.MyFishId)[j]],
                allFishRoot);
            fishTransform.localPosition = FishRelativePosition(enemy, j);
            fishTransform.localScale = Small;
            fishTransform.rotation = Quaternion.Euler(new Vector3(0, enemy ? 100 : 260, 0));
            if (SharedRefs.Mode == Constants.GameMode.Offline) return fishTransform;

            var fishTrigger = new EventTrigger.Entry();
            fishTrigger.callback.AddListener(delegate
            {
                switch (_gameStates.GameStatus)
                {
                    case Constants.GameStatus.DoAssertion:
                        if (enemy) _gameStates.Assertion = _gameStates.Assertion == j ? -1 : j;
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

        public void Init(Transform unkFishPrefab, Transform allFishRoot)
        {
            for (var i = 0; i < 4; i++)
            {
                var myFish = GenFish(false, i, unkFishPrefab, allFishRoot);
                MyFishTransforms.Add(myFish);
                MyFishMeshRenderers.Add(myFish.GetComponentsInChildren<SkinnedMeshRenderer>());
                MyFishParticleSystems.Add(myFish.GetComponentInChildren<ParticleSystem>());

                var enemyFish = GenFish(true, i, unkFishPrefab, allFishRoot);
                EnemyFishTransforms.Add(enemyFish);
                EnemyFishMeshRenderers.Add(enemyFish.GetComponentsInChildren<SkinnedMeshRenderer>());
                EnemyFishParticleSystems.Add(enemyFish.GetComponentInChildren<ParticleSystem>());
            }
            Initialized = true;
        }

        public GameObjectManager(GameStates gameStates)
        {
            _gameStates = gameStates;
        }
    }
}