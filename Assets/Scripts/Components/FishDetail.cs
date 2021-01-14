using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Components
{
    public class FishDetail : MonoBehaviour
    {
        public Image avatar;

        public Text text;

        public void SetupFish(int id)
        {
            text.text = Constants.FishName[id];
            avatar.overrideSprite = SharedRefs.FishAvatars[id];
        }
    }
}