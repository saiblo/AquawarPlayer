using UnityEngine;
using UnityEngine.UI;

namespace Components
{
    public class Hp : MonoBehaviour
    {
        public int Current { get; set; }

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

        private Slider _slider;
        private int _full;

        private void Awake()
        {
            _slider = GetComponent<Slider>();
        }

        private void Update()
        {
            text.text = $"{Current}/{Full}";
            _slider.value = (float) Current / Full;
            if (Current <= 0) gameObject.SetActive(false);
        }
    }
}