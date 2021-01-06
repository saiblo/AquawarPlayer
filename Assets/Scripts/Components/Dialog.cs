using UnityEngine;

// https://www.cnblogs.com/wjr0117/p/9804341.html

namespace Components
{
    public class Dialog : MonoBehaviour
    {
        private static Vector3 _vec3;
        private static Vector3 _pos;

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        public void PointerDown()
        {
            _vec3 = Input.mousePosition;
            _pos = transform.GetComponent<RectTransform>().position;
        }

        public void Drag()
        {
            var off = Input.mousePosition - _vec3;
            _vec3 = Input.mousePosition;
            _pos += off;
            transform.GetComponent<RectTransform>().position = _pos;
        }

        public void OnShow()
        {
            gameObject.SetActive(true);
        }

        public void OnOK()
        {
            gameObject.SetActive(false);
        }
    }
}