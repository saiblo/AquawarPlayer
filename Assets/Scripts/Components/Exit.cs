using UnityEditor;
using UnityEngine;

namespace Components
{
    public class Exit : MonoBehaviour
    {
        public void DoExitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}