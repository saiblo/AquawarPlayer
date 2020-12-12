using UnityEngine;

public class Underwater : MonoBehaviour
{
    private void Awake()
    {
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.1f, 0.13f, 0.36f, 0.8f);
        RenderSettings.fogDensity = 0.01f;
    }
}