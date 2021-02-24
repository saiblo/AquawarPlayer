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
        public Text passive;
        public Image avatar;

        private MonoBehaviour _extension;

        public void SetupFish(int id, MonoBehaviour extension, int imitate = -1)
        {
            fishName.text = imitate == -1 ? Constants.FishName[id] : $"<{Constants.FishName[imitate]}>";
            skill.text = Constants.SkillTable[id];
            if (passive != null) passive.text = Constants.PassiveTable[id];
            avatar.overrideSprite = SharedRefs.FishAvatars[id];
            _extension = extension;
        }

        public void SetHp(int hpVal)
        {
            hp.text = $"Hp: {hpVal}";
        }

        public void SetAtk(int atkVal)
        {
            atk.text = atkVal == -1 ? "Atk: ???" : $"Atk: {atkVal}";
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