using UnityEngine;
using UnityEngine.UI;

namespace Components
{
    public class FullScreen : MonoBehaviour
    {
        private bool _fullScreen = true;

        public Text prompt;

        public void Toggle()
        {
            _fullScreen = !_fullScreen;
            Screen.fullScreen = _fullScreen;
        }

        private void Awake()
        {
            Screen.fullScreen = true;
        }

        private void Update()
        {
            prompt.text = _fullScreen ? "退出全屏" : "进入全屏";
        }
    }
}