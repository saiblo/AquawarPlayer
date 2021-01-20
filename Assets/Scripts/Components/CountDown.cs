using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Components
{
    public class CountDown : MonoBehaviour
    {
        private Text _title;

        private int _time = 60;

        public void StartTiming(Text title)
        {
            _title = title;
            StartCoroutine(Time());
        }

        private IEnumerator Time()
        {
            while (_time >= 0)
            {
                _title.text = $"倒计时：{_time}";
                yield return new WaitForSeconds(1);
                --_time;
            }
        }
    }
}