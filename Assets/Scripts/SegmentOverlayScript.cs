using UnityEngine;
using System.Collections;

public class SegmentOverlayScript : MonoBehaviour {

	public Color shining, deactivated;
	private float animTime = 0.75f;
	private float timer = 0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		timer -= Time.deltaTime;
		GetComponent<Renderer>().material.color = Color.Lerp(shining, deactivated, 1f - (timer/animTime));
	}

	public void Shine(){
		timer = animTime;
	}
}
