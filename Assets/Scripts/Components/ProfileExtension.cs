using UnityEngine;
using UnityEngine.UI;

namespace Components
{
    public class ProfileExtension : MonoBehaviour
    {
        public Text text;

        public void UpdateText(string newText)
        {
            text.text = newText;
        }
    }
}