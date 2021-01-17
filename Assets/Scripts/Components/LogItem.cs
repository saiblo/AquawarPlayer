using UnityEngine;
using UnityEngine.UI;

namespace Components
{
    public class LogItem : MonoBehaviour
    {
        public Text text;
        
        public void SetText(string newText)
        {
            text.text = newText;
        }
    }
}