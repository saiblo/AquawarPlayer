using UnityEngine;
using UnityEngine.UI;

namespace Components
{
    public class Hp : MonoBehaviour
    {
        public int Current
        {
            get => _current;
            set
            {
                _current = value;
                text.text = $"{value}";
                slider.value = (float) value / Full;
                profile.SetHp(value);
                gameObject.SetActive(value > 0);
            }
        }

        public int Full
        {
            get => _full;
            set
            {
                _full = value;
                Current = value;
            }
        }

        public Text text;
        public Slider slider;

        public GameProfile profile;

        private int _full;
        private int _current;
    }
}