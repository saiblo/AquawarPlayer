using LitJson;
using UnityEngine;

namespace Utils
{
    public static class SharedRefs
    {
        public static readonly Transform[] FishPrefabs = new Transform[Constants.FishNum];

        public static readonly Sprite[] FishAvatars = new Sprite[Constants.FishNum];

        public static readonly bool[] MyFishIdExpose = new bool[Constants.FishNum];

        public static readonly bool[] EnemyFishIdExpose = new bool[Constants.FishNum];

        public static int ReplayCursor = 0;

        public static JsonData ReplayJson = null;

        public static bool AutoPlay = false;

        public static JsonData ActionInfo = null; // Do not want to describe it. Shared between states. See the code.

        public static int MyImitate = 11;

        public static int EnemyImitate = 11;

        public static bool ErrorFlag = false; // Indicates whether reading an error record
    }
}