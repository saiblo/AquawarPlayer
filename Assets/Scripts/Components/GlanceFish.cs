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
        public Image border;

        private int _id;
        private ProfileExtension _extension;

        public bool detailed;

        public void SetupFish(int id, Constants.FishState state, ProfileExtension extension)
        {
            fishAvatar.overrideSprite = SharedRefs.FishAvatars[id];
            switch (state)
            {
                case Constants.FishState.Used:
                    mask.color = new Color(1, 0, 0, 0.3f);
                    border.color = Color.grey;
                    break;
                case Constants.FishState.Using:
                    mask.color = new Color(0, 0, 0, 0);
                    border.color = Color.white;
                    break;
                case Constants.FishState.Free:
                    mask.color = new Color(0, 0, 0, 0.6f);
                    border.color = new Color(0, 128, 0, 0.6f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
            _extension = extension;
            _id = id;
        }

        public void ShowExt()
        {
            _extension.UpdateText(
                $"{Constants.FishName[_id]}\n主动：{(detailed ? Constants.SkillDescription : Constants.SkillTable)[_id]}\n被动：{(detailed ? Constants.PassiveDescription : Constants.PassiveTable)[_id]}"
            );
            _extension.gameObject.SetActive(true);
        }

        public void HideExt()
        {
            _extension.gameObject.SetActive(false);
        }
    }
}