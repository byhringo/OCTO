using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class IngameMenuScript : MonoBehaviour {

	public Text timer;
	public Image panelBG;
	public GameObject restartButton, mainMenuButton;
	public Color normalColor, deadColor;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetTimerText(string t){
		timer.text = t;
	}

	public void NotifyDead(){
		panelBG.color = deadColor;
		restartButton.SetActive(true);
		mainMenuButton.SetActive(true);
	}

	public void Reset(){
		panelBG.color = normalColor;
		restartButton.SetActive(false);
		mainMenuButton.SetActive(false);
	}
}
