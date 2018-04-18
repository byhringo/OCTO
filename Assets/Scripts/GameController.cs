using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

	public static float bpm = 174f;

	public GameObject startMenuPanel, ingamePanel, pausePanel;
	public Button mediumButton, hardButton, mediumDeactivatedButton, hardDeactivatedButton;
	public Text easyHS, mediumHS, hardHS;

	public Material[] colors;

	public AudioClip[] sounds;

	public AudioClip[] music;

	private AudioSource sfx;
	private AudioLowPassFilter lowPass;

	public SpawnScript spawner;
	public OctagonScript octagon;
	public IngameMenuScript igm;

	public CameraSHAKE camShake;
	public TimerScript timer;

	private OrbScript lastOrb = null;

	private int level = 1;
	private int[] levelDifficultyOffsets = {0, 8, 20}; //Bad but easy solution to dumb problem caused by bad design early on

	//1 = Easy
	//2 = Medium
	//3 = Hard
	private int difficulty = 1;

	//How much do we spin the camera?
	private int spinLevel = 0;

	/* 0 - In start menu
	 * 1 - Ingame, playing
	 * 2 - Showing end of game screen
	 * 3 - Paused
	 */
	private int state = 0;

	private float startTime = 0;
	private float timerValue = 0;
	private float pauseTimer = 0; //How long have we spent paused?

	private float pitchTarget = 1f;
	private float lowPassCutoffTarget = 22000f;

	// Use this for initialization
	void Start () {
		sfx = GetComponent<AudioSource>();
		lowPass = GetComponent<AudioLowPassFilter>();
		SetupUI();
	}

	private void SetupUI(){
		int unlocked = PlayerPrefs.GetInt("UnlockedLevels", 3);
		if(unlocked == 3){
			mediumButton.gameObject.SetActive(true);
			hardButton.gameObject.SetActive(true);

			mediumDeactivatedButton.gameObject.SetActive(false);
			hardDeactivatedButton.gameObject.SetActive(false);
		}
		else if(unlocked == 2){
			if(unlocked == 3){
				mediumButton.gameObject.SetActive(true);

				mediumDeactivatedButton.gameObject.SetActive(false);
			}
		}

		SetupDifficultyButtons();
	}
	
	// Update is called once per frame
	void Update () {
		if(state == 1){
			timerValue = Time.time - startTime;
			float gameTime = timerValue - pauseTimer;
			int minutes = Mathf.FloorToInt(gameTime / 60);
			int seconds = Mathf.FloorToInt(gameTime - (60*minutes));
			int millis = Mathf.FloorToInt((gameTime - (seconds + (60*minutes))) * 100);
			igm.SetTimerText(
				(minutes > 0 ? (minutes + ":") : "")
				+ ((minutes > 0 && seconds < 10) ? "0" : "") + seconds + ":"
				+ (millis < 10 ? "0" + millis : "" + millis));
		}
		else if(state == 2){
			if(Input.GetKeyDown(KeyCode.Space)){
				RestartGame();
			}
		}
		else if(state == 3){
			pauseTimer += Time.deltaTime;
		}

		sfx.pitch = Mathf.Lerp(sfx.pitch, pitchTarget, 0.1f);
		lowPass.cutoffFrequency = Mathf.Lerp(lowPass.cutoffFrequency, lowPassCutoffTarget, 10f*Time.deltaTime);
	}

	public void RestartGame(){
		foreach(GameObject o in GameObject.FindGameObjectsWithTag("Orb")){
			Destroy(o);
		}

		startMenuPanel.SetActive(false);
		ingamePanel.SetActive(true);

		state = 1;

		level = 1;

		sfx.pitch = 1f;
		pitchTarget = 1f;
		lowPassCutoffTarget = 22000f;

		/*
		if(difficulty == 2){
			level = 8; //TODO: When should we start?
		} else if(difficulty == 3){
			level = 20; //TODO: When should we start?
		}*/

		spawner.NewGameStarted(level + levelDifficultyOffsets[difficulty-1]);
		octagon.SetState(1);
		igm.Reset();
		timerValue = 0;
		pauseTimer = 0;

		spinLevel = 0;
		camShake.ResetRotation();
		timer.SetGlowing(false);

		PlayRandomMusic();
		startTime = Time.time;
	}

	public Material getMaterial(int c){
		return c >= 0 ? colors[c] : colors[8];
	}

	public float GetCollisionDistance(){
		return 8.25f;
	}

	public void PlayRandomMusic(){
		sfx.clip = music[Random.Range(0, music.Length)];
		sfx.Play();
	}

	public void RegisterOrbCollision(OrbScript os){
		int segmentIndex = (int)Mathf.Round((os.GetAngle() - 22.5f)/45f);
		segmentIndex -= (int)Mathf.Round(octagon.GetRotation()/45f);
		if(segmentIndex < 0){
			segmentIndex += 8;
		}
		segmentIndex %= 8;


		int shouldHit = octagon.GetColorForSegment(segmentIndex);

		if(shouldHit == os.GetColor()){
			TriggerCorrect(segmentIndex);
			os.GoAway();
			if(os == lastOrb){
				octagon.SetState(2);

				camShake.ShakeItUp(0.2f, 3f);
			}
		}
		else{
			TriggerMistake(os);
		}
	}

	private void TriggerMistake(OrbScript os){
		state = 2;
		octagon.SetState(4);
		spawner.SetPaused(true);

		foreach(GameObject o in GameObject.FindGameObjectsWithTag("Orb")){
			o.GetComponent<OrbScript>().SetPaused(true);
		}

		igm.NotifyDead();

		if(RegisterScore(timerValue - pauseTimer)){
			timer.SetGlowing(true);

			SetupDifficultyButtons();
		}

		camShake.ShakeItUp(0.75f, 1.5f);

		//sfx.Stop();
		//pitchTarget = 0.35f;
		lowPassCutoffTarget = 500f;
		SFXDie();
	}

	private void TriggerCorrect(int i){
		octagon.ShineOverlay(i);
		camShake.ShakeItUp(0.1f, 1.5f);
		SFXCorrect();
	}

	public int GetAvailableColorCount(){
		return colors.Length-1;
	}

	public int GetSpinLevel(){
		return spinLevel;
	}

	public bool IsPaused(){
		return state == 3;
	}

	public void SetSegmentColors(int[] activeColors){
		octagon.SetSegmentColors(activeColors);
	}

	public void DoneSpawning(OrbScript lo){
		lastOrb = lo;
	}

	public void AnimationStage1Complete(){
		level++;
		spawner.InitiateNewSetup(level + levelDifficultyOffsets[difficulty-1]);
		spawner.SetPaused(true);
	}

	public void AnimationStage2Complete(){
		spawner.SetPaused(false);
		octagon.SetState(1);
		camShake.ShakeItUp(0.2f, 1f);

		//Difficulties start at lvl 1, 8 and 20
		if(level > 11 - levelDifficultyOffsets[difficulty-1]){
			spinLevel++;
			camShake.StartSpinning();
		}
	}

	public void PauseGame(){
		if(state == 1){
			state = 3;
			octagon.SetPaused(true);
			spawner.SetPaused(true);

			foreach(GameObject o in GameObject.FindGameObjectsWithTag("Orb")){
				o.GetComponent<OrbScript>().SetPaused(true);
			}
			ingamePanel.SetActive(false);
			pausePanel.SetActive(true);
			sfx.Pause();
		}
	}

	public void UnpauseGame(){
		state = 1;
		octagon.SetPaused(false);
		spawner.SetPaused(false);

		foreach(GameObject o in GameObject.FindGameObjectsWithTag("Orb")){
			o.GetComponent<OrbScript>().SetPaused(false);
		}
		ingamePanel.SetActive(true);
		pausePanel.SetActive(false);
		sfx.UnPause();
	}

	public void StartGameEasy(){
		difficulty = 1;
		RestartGame();
	}

	public void StartGameMedium(){
		difficulty = 2;
		RestartGame();
	}

	public void StartGameHard(){
		difficulty = 3;
		RestartGame();
	}

	public void BackToMainMenu(){
		if(state == 2){
			state = 0;
			ingamePanel.SetActive(false);
			startMenuPanel.SetActive(true);
			sfx.Stop();
		}
	}

	public float GetTimePassed(){
		return timerValue - pauseTimer;
	}

	public void SFXSpawn(){
		sfx.PlayOneShot(sounds[0]);
	}

	public void SFXCorrect(){
		sfx.PlayOneShot(sounds[1]);
	}

	public void SFXDie(){
		sfx.PlayOneShot(sounds[2]);
	}

	public bool RegisterScore(float timePlayed){
		string dif = "easy";
		if(difficulty == 2){
			dif = "medium";
		}
		if(difficulty == 3){
			dif = "hard";
		}

		float currentHS = PlayerPrefs.GetFloat(dif+"HS", 0);

		if(timePlayed > currentHS){
			PlayerPrefs.SetFloat(dif + "HS", timePlayed);
			return true;
		}
		return false;
	}

	private void SetupDifficultyButtons(){
		//Easy button highscore
		float easyHighscore = PlayerPrefs.GetFloat("easyHS", 0);
		int minutes = Mathf.FloorToInt(easyHighscore / 60);
		int seconds = Mathf.FloorToInt(easyHighscore - (60*minutes));
		int millis = Mathf.FloorToInt((easyHighscore - (seconds + (60*minutes))) * 100);
		easyHS.text = 
			(minutes > 0 ? (minutes + ":") : "")
			+ ((minutes > 0 && seconds < 10) ? "0" : "") + seconds + ":"
			+ (millis < 10 ? "0" + millis : "" + millis);

		if(minutes >= 1){
			mediumButton.gameObject.SetActive(true);
			mediumDeactivatedButton.gameObject.SetActive(false);

			//Medium button highscore
			float mediumHighscore = PlayerPrefs.GetFloat("mediumHS", 0);
			minutes = Mathf.FloorToInt(mediumHighscore / 60);
			seconds = Mathf.FloorToInt(mediumHighscore - (60*minutes));
			millis = Mathf.FloorToInt((mediumHighscore - (seconds + (60*minutes))) * 100);
			mediumHS.text = 
			(minutes > 0 ? (minutes + ":") : "")
			+ ((minutes > 0 && seconds < 10) ? "0" : "") + seconds + ":"
			+ (millis < 10 ? "0" + millis : "" + millis);

			if(minutes >= 1){
				hardButton.gameObject.SetActive(true);
				hardDeactivatedButton.gameObject.SetActive(false);

				//Hard button highscore
				float hardHighscore = PlayerPrefs.GetFloat("hardHS", 0);
				minutes = Mathf.FloorToInt(hardHighscore / 60);
				seconds = Mathf.FloorToInt(hardHighscore - (60*minutes));
				millis = Mathf.FloorToInt((hardHighscore - (seconds + (60*minutes))) * 100);
				hardHS.text = 
					(minutes > 0 ? (minutes + ":") : "")
					+ ((minutes > 0 && seconds < 10) ? "0" : "") + seconds + ":"
					+ (millis < 10 ? "0" + millis : "" + millis);
			}
			else{
				//Hard button deactivated
				hardButton.gameObject.SetActive(false);
				hardDeactivatedButton.gameObject.SetActive(true);
			}
		}
		else{
			//Hard and medium button deactivated
			mediumButton.gameObject.SetActive(false);
			hardButton.gameObject.SetActive(false);

			mediumDeactivatedButton.gameObject.SetActive(true);
			hardDeactivatedButton.gameObject.SetActive(true);
		}
	}
}
