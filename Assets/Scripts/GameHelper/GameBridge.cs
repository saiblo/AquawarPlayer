using UnityEngine;
using UnityEngine.UI;

namespace GameHelper
{
    /// <summary>
    ///   <para>Lists all member properties that have to be exposed to the scene.</para>
    /// </summary>
    public abstract class GameBridge : EnhancedMonoBehaviour
    {
        // Prefabs

        public Transform unkFishPrefab;

        public Transform statusBarPrefab;

        public Transform questionPrefab;

        // Particle system

        public Transform waterProjectile;

        public Transform explodePrefab;
        public Transform bigExplosion;
        public Transform recoverEffect;
        public Transform shieldEffect;

        public Material dissolveEffect;
        public AnimationCurve fadeIn;

        // Root transforms

        public Transform allFishRoot;
        public Transform myStatusRoot;
        public Transform enemyStatusRoot;

        // UI objects

        public Text logText;
    }
}