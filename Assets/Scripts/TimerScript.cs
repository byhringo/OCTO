using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TimerScript : MonoBehaviour {

	private bool glowing = false;
	private float glowStart = 0f;

	public Text t;
	public Color baseColor, glowColor;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(glowing){
			float animVal = Mathf.Sin((Time.time - glowStart)*7f)/10f; //from -0.1 to 0.1

			transform.localScale = Vector3.one * (1f + animVal);
			t.color = Color.Lerp(baseColor, glowColor, (animVal + 0.1f)*5f);
		}
		else{
			transform.localScale = Vector3.one;
			t.color = baseColor;
		}
	}

	public void SetGlowing(bool g){
		glowing = g;
		glowStart = Time.time;
	}
}
