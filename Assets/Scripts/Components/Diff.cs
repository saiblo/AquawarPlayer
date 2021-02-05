using GameHelper;
using UnityEngine;
using UnityEngine.UI;

namespace Components
{
    public class Diff : EnhancedMonoBehaviour
    {
        public Text text;

        public void Setup(int value, bool positive)
        {
            text.text = $"{(positive ? "+" : "-")}{value}";
            text.color = positive ? Color.blue : Color.red;
            text.gameObject.SetActive(true);
            transform.localPosition += new Vector3(0, 50, 0);
            Repeat(cnt =>
            {
                transform.localPosition += new Vector3(0, 5, 0);
            }, () =>
            {
                Destroy(gameObject);
            },20,0,40);
        }

        protected override void RunPerFrame()
        {
        }
    }
}