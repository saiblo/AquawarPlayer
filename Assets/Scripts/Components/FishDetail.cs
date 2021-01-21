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
            text.text =
                $"{Constants.FishName[id]}\n\n主动技能：{Constants.SkillDescription[id]}\n\n被动技能：{Constants.PassiveDescription[id]}";
            avatar.overrideSprite = SharedRefs.FishAvatars[id];
        }
    }
}