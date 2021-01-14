using Components;
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

        // Particle system

        public Transform waterProjectile;

        public Transform explodePrefab;
        public Transform bigExplosion;
        public Transform recoverEffect;
        public Transform shieldEffect;

        public Material dissolveEffect;
        public AnimationCurve fadeIn;

        public Transform fogPrefab;

        // Root transforms

        public Transform allFishRoot;

        // UI objects

        public Text logText;

        public Hp[] myStatus;
        public Hp[] enemyStatus;

        public Text roundText;
        public Text scoreText;
        public Text resultText;
        public Button doneNextRoundButton;

        public GameObject logObject;

        public GameProfile[] myProfiles;
        public GameProfile[] enemyProfiles;

        public ProfileExtension[] myExtensions;
        public ProfileExtension[] enemyExtensions;

        public ProfileExtension myGlanceExt;
        public ProfileExtension enemyGlanceExt;

        public Glance myGlance;
        public Glance enemyGlance;

        public Glance assertionModal;
        public ProfileExtension assertionExt;

        public Text playButtonText;
        public Button prevStepButton;
        public Button nextStepButton;
        public Button replayStepButton;
        public Button prevRoundButton;
        public Button nextRoundButton;

        public GameObject assertionButtons;
        public GameObject attackButtons;
        public Button normalAttackButton;
        public Button skillAttackButton;
        public Button confirmAttackButton;
    }
}