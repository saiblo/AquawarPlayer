using UnityEngine;
using System.Collections;

public class blendShapesPufferFish : MonoBehaviour {

	public float my_slider = 0.0F;

	private SkinnedMeshRenderer skinMeshRenderer;

	// Use this for initialization
	void Start () {

		skinMeshRenderer = GetComponent<SkinnedMeshRenderer>();
	
	}

	void OnGUI() {
		GUI.contentColor = Color.black;
		my_slider = GUI.HorizontalSlider(new Rect(50,150,100,30), my_slider, 0.0F,100.0F);
		GUI.Label (new Rect(50,125,100,20), "Blend Shape");
	}
	
	// Update is called once per frame
	void Update () {

		skinMeshRenderer.SetBlendShapeWeight(0,my_slider);
	
	}
}
