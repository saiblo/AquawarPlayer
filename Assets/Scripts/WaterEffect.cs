using UnityEngine;

public class WaterEffect : MonoBehaviour
{
    public float fps = 30.0f;
    public Texture2D[] frames; //caustics images

    private int _frameIndex;
    private Projector _projector; //Projector GameObject
    private static readonly int ShadowTex = Shader.PropertyToID("ShadowTex");

    private void Awake()
    {
        _projector = GetComponent<Projector>();
        NextFrame();
        InvokeRepeating(nameof(NextFrame), 1 / fps, 1 / fps);
    }

    private void NextFrame()
    {
        _projector.material.SetTexture(ShadowTex, frames[_frameIndex]);
        _frameIndex = (_frameIndex + 1) % frames.Length;
    }
}