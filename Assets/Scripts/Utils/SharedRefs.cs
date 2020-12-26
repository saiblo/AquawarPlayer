using LitJson;
using UnityEngine;

namespace Utils
{
    public static class SharedRefs
    {
        public static readonly Transform[] FishPrefabs = new Transform[Constants.FishNum];

        public static int ReplayCursor = 0;

        public static JsonData ReplayJson = null;

        public static Constants.GameMode Mode = Constants.GameMode.Online;

        public static Client GameClient = null;
    }
}