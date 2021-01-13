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
                text.text = $"{value}/{Full}";
                slider.value = (float) value / Full;
                gameObject.SetActive(value > 0);
            }
        }

        public int Full
        {
            get => _full;
            set
            {
                Current = value;
                _full = value;
            }
        }

        public Text text;
        public Slider slider;

        private int _full;
        private int _current;
    }
}