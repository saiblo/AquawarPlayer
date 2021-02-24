using UnityEngine;
using Utils;

namespace Components
{
    public class Glance : MonoBehaviour
    {
        public GlanceFish glanceFishPrefab;

        public int hDist;
        public int vDist;
        public int vBias;

        public GlanceFish[] allGlanceFish = {null, null, null, null, null, null, null, null, null, null, null, null};

        public void SetupFish(int id, Constants.FishState state, ProfileExtension extension, GameUI gameUI = null)
        {
            var fish = Instantiate(glanceFishPrefab, gameObject.transform);
            fish.GetComponent<Transform>().localPosition
                = new Vector3((id % 4 - 1.5f) * hDist, (1 - id / 4) * vDist - vBias);
            fish.SetupFish(id, state, extension);
            fish.detailed = gameUI != null;
            allGlanceFish[id] = fish;
        }
    }
}