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

        public static Constants.GameMode Mode = Constants.GameMode.Online;

        public static bool AutoPlay = false;

        public static Client GameClient = null;

        public static JsonData FirstPick = null; // The first message from remote, shared by Welcome and Preparation

        public static List<int> FishChosen = null; // Shared by Preparation and Game
    }
}