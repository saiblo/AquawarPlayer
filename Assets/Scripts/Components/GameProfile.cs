using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Components
{
    public class GameProfile : MonoBehaviour
    {
        public Text fishName;

        public void SetupFish(int id)
        {
            fishName.text = Constants.FishName[id];
        }
    }
}