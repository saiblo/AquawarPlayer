﻿using Components;
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

        public Transform[] fishPrefabSamples;

        public Sprite[] fishAvatars;

        // Particle system

        public Transform waterProjectile;

        public Transform explodePrefab;
        public Transform bigExplosion;
        public Transform recoverEffect;
        public Transform shieldEffect;

        public Material dissolveEffect;
        public AnimationCurve fadeIn;

        public Transform fogPrefab;

        public Transform smallExplosion;

        public Transform fireBallPrefab;

        // Root transforms

        public Transform allFishRoot;

        // Sprites

        public Sprite darkBlue;
        public Sprite lightBlue;

        public Sprite playIcon;
        public Sprite pauseIcon;

        // UI objects

        public bool logActive;
        public GameObject logObject;

        public Hp[] myStatus;
        public Hp[] enemyStatus;

        public Diff diffPrefab;

        public Text roundText;
        public Text scoreText;
        public Text resultText;
        public Text gameOverText;
        public GameObject gameOverMask;

        public Transform logContent;
        public LogItem logItem;

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

        public GameObject offlineOnlyButtons;
        public GameObject questionButton;
        public GameObject hintImage;
        public Text hintText;
        public Image playButtonImage;
        public Button prevStepButton;
        public Button nextStepButton;
        public Button prevRoundButton;
        public Button nextRoundButton;

        public GameObject[] assertionButtons;
        public GameObject doNotAssertButton;
        public GameObject confirmActionGroup;
        public Button confirmAttackButton;

        public ActionButtons[] actionButtons;

        public CountDown countDownPrefab;
        public GameObject exitConfirmMask;

        public Text[] counters;

        // Misc
        public Text playerNamesText;
    }
}