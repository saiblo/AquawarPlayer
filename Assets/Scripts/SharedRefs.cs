using LitJson;
using UnityEngine;

public static class SharedRefs
{
    public static readonly Transform[] FishPrefabs = new Transform[Constants.FishNum];

    public static int ReplayCursor = 0;

    public static JsonData ReplayJson = null;
}