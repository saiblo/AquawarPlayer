using System.Collections.Generic;
using LitJson;
using UnityEngine;

namespace Utils
{
    public static class SharedRefs
    {
        public static readonly Transform[] FishPrefabs = new Transform[Constants.FishNum];

        public static readonly Sprite[] FishAvatars = new Sprite[Constants.FishNum];

        public static int ReplayCursor = 0;

        public static JsonData ReplayJson = null;

        public static string OnlineToken = null;

        public static string PlayerNames = null;

        public static Constants.GameMode Mode = Constants.GameMode.Online;

        public static int OnlineWaiting = 0;

        public static string PendingMessage = null; // Shared by Preparation and Game

        public static bool AutoPlay = false;

        public static Client GameClient = null;

        public static JsonData PickInfo = null;

        public static List<int> FishChosen = null; // Shared by Preparation and Game

        public static JsonData ActionInfo = null; // Do not want to describe it. Shared between states. See the code.

        public static int MyImitate = 11;

        public static int EnemyImitate = 11;

        public static bool ErrorFlag = false; // Indicates whether reading an error record

        public static int OnlineWin = 0;
        public static int OnlineLose = 0;
    }
}