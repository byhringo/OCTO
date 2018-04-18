using UnityEngine;
using System.Collections;

public class BGSegmentScript : MonoBehaviour {

	private GameController gc;
	private Color initColor;
	private Renderer r;

	// Use this for initialization
	void Start () {
		r = GetComponent<Renderer>();
		initColor = r.material.color;
	}

	// Update is called once per frame
	void Update () {
		r.material.color = Color.Lerp(r.material.color, initColor, 0.1f);
	}

	public void SetGC(GameController gameControl){
		gc = gameControl;
	}

	public void SetColor(int colorID){
		r.material.color = Color.Lerp(initColor, gc.getMaterial(colorID).color, 0.5f);
	}
}
