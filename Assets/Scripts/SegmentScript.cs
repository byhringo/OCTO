using UnityEngine;
using System.Collections;

public class SegmentScript : MonoBehaviour {

	private GameController gc;
	private int currentColor;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void SetGC(GameController gameControl){
		gc = gameControl;
	}

	public void SetColor(int colorID, Material m){
		GetComponent<Renderer>().material = m;
		currentColor = colorID;
	}

	public int GetColor(){
		return currentColor;
	}
}
