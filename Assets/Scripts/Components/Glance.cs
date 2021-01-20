using GameImpl;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;

namespace Components
{
    public class Glance : MonoBehaviour
    {
        public GlanceFish glanceFishPrefab;

        public int hDist;
        public int vDist;
        public int vBias;

        public void SetupFish(int id, Constants.FishState state, ProfileExtension extension, GameUI gameUI = null)
        {
            var fish = Instantiate(glanceFishPrefab, gameObject.transform);
            fish.GetComponent<Transform>().localPosition
                = new Vector3((id % 4 - 1.5f) * hDist, (1 - id / 4) * vDist - vBias);
            fish.SetupFish(id, state, extension);

            if (!gameUI) return;
            var fishTrigger = new EventTrigger.Entry();
            fishTrigger.callback.AddListener(delegate
            {
                gameUI.GameState.AssertionTarget = id;
                gameUI.CloseAssertionModal();
                gameUI.doNotAssertButton.SetActive(false);
                gameUI.ChangeStatus();
            });
            fish.fishAvatar.GetComponent<EventTrigger>().triggers.Add(fishTrigger);
        }
    }
}