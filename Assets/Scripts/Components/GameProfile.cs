using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Components
{
    public class GameProfile : MonoBehaviour
    {
        public Text fishName;
        public Text hp;
        public Text atk;
        public Text skill;
        public Image avatar;

        private MonoBehaviour _extension;

        public void SetupFish(int id, MonoBehaviour extension)
        {
            fishName.text = Constants.FishName[id];
            skill.text = Constants.SkillTable[id];
            avatar.overrideSprite = SharedRefs.FishAvatars[id];
            _extension = extension;
        }

        public void SetHp(int hpVal)
        {
            hp.text = $"Hp: {hpVal}";
        }

        public void SetAtk(int atkVal)
        {
            atk.text = $"Atk: {atkVal}";
        }

        public void ShowExt()
        {
            _extension.gameObject.SetActive(true);
        }

        public void HideExt()
        {
            _extension.gameObject.SetActive(false);
        }
    }
}