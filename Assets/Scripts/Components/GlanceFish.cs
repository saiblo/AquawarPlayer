using System;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Components
{
    public class GlanceFish : MonoBehaviour
    {
        public Text fishName;
        public Text fishState;

        public void SetupFish(int id, Constants.FishState state)
        {
            fishName.text = Constants.FishName[id];
            switch (state)
            {
                case Constants.FishState.Used:
                    fishState.text = "已使用";
                    break;
                case Constants.FishState.Using:
                    fishState.text = "在场上";
                    break;
                case Constants.FishState.Free:
                    fishState.text = "未使用";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
            
        }

    }
}