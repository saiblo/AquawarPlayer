using System;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Components
{
    public class GlanceFish : MonoBehaviour
    {
        public Image fishAvatar;
        public Image mask;

        public void SetupFish(int id, Constants.FishState state)
        {
            fishAvatar.overrideSprite = SharedRefs.FishAvatars[id];
            switch (state)
            {
                case Constants.FishState.Used:
                    mask.color = new Color(1, 0, 0, 0.4f);
                    break;
                case Constants.FishState.Using:
                    mask.color = new Color(0, 0, 0, 0.4f);
                    break;
                case Constants.FishState.Free:
                    mask.color = new Color(0, 1, 0, 0.4f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
}