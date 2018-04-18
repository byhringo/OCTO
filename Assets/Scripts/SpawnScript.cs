using UnityEngine;
using System.Collections;

public class SpawnScript : MonoBehaviour {
	//TODO: Remove if always set to 1
	private static int beatsPerSpawn = 1;
	private static int delay = 4;

	public GameController gc;

	public GameObject orbPrefab;

	private float spawnRate;
	private float orbSpeed;
	private int[] activeColors;
	private int availableColorCount;
	private int spawnCount = 0;
	private int patternIndex = 0;
	private bool paused = true;

	private int previousBeatCount;

	//Template: new bool[] {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
	private bool[][] spawnPatternsEasy = {
		new bool[] {true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false},	// 4
		new bool[] {true, false, false, false, true, false, false, false, true, false, false, false, true, false, true, false},		// 5
		new bool[] {true, false, false, false, true, false, true, false, true, false, false, false, true, false, true, false},		// 6
		new bool[] {true, false, false, false, true, false, false, false, true, true, false, true, true, false, true, false},		// 7
	};

	private bool[][] spawnPatternsMedium = {
		new bool[] {true, true, false, false, true, true, false, false, true, true, false, false, true, true, false, false},		// 8
		new bool[] {true, false, true, false, true, false, true, false, true, false, false, true, false, false, false, true},		// 7
		new bool[] {true, true, false, false, true, true, false, false, true, false, true, false, true, false, true, false},		// 8
		new bool[] {true, false, true, true, true, false, false, true, true, false, true, true, false, false, true, false},			// 9
	};

	private bool[][] spawnPatternsHard = {
		new bool[] {true, true, false, true, true, true, true, false, true, false, true, true, false, true, true, false},			// 11
		new bool[] {true, true, true, false, true, true, true, false, true, true, true, false, true, true, true, false},			// 12
		new bool[] {true, true, true, true, true, false, true, false, true, true, true, true, true, false, true, false},			// 12
		new bool[] {true, true, false, true, true, false, true, true, true, false, true, true, true, true, true, false},			// 12
	};

	private bool[] activePattern;

	// Use this for initialization
	void Start () {
		spawnRate = GameController.bpm/(60f*beatsPerSpawn);
		orbSpeed = (8.45f*spawnRate)/4.5f; //Math is hard - I don't remember why this works
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		//First we figure out how many beats have passed
		int beatCount = ((Mathf.FloorToInt(gc.GetTimePassed()/(60f/GameController.bpm)) - delay)/beatsPerSpawn);

		if(previousBeatCount < beatCount){
			patternIndex = (patternIndex + 1) % activePattern.Length;

			if(activePattern[patternIndex] && !paused && spawnCount > 0){
				SpawnOrb();
			}

			previousBeatCount = beatCount;
		}
	}

	public void NewGameStarted(int startLevel){
		InitiateNewSetup(startLevel);
		SetPaused(false);
		previousBeatCount = 0;
	}

	public void InitiateNewSetup(int level){
		//Enable 2 colors at level 2, 3 colors at lvl 10, turn off 1 color at lvl 20
		int colorCount = 1;

		if(level > 19){
			colorCount = Random.Range(2, 4);
		} else if(level > 9){
			colorCount = Random.Range(1, 4);
		} else if(level > 1){
			colorCount = Random.Range(1, 3);
		}

		//Set the complexity of the pattern
		//Enable medium after lvl 4, enable hard after lvl 8. For each color after 1, reduce utilized level value by 5
		//If we have 3 colors, don't use the hardest patterns!
		int patternComplexity = 0;

		if(level - ((colorCount - 1)*5) > 7){
			patternComplexity = (colorCount == 3 ? 1 : 2);
		} else if(level - ((colorCount - 1)*5) > 3){
			patternComplexity = 1;
		}

		bool[][] chosenPatternGroup;

		switch(patternComplexity){
		case 0: 	chosenPatternGroup = spawnPatternsEasy;		break;
		case 1: 	chosenPatternGroup = spawnPatternsMedium;	break;
		case 2: 	chosenPatternGroup = spawnPatternsHard;		break;
		default: 	chosenPatternGroup = spawnPatternsEasy;		break;
		}

		availableColorCount = gc.GetAvailableColorCount();
		activeColors = CreateRandomColorSetup(colorCount, availableColorCount);

		int chosenPattern = Mathf.Min(level-1, Random.Range(0, chosenPatternGroup.Length));

		//Hard coded exception so we don't get the two simplest patterns after level 5
		if(level > 5 && chosenPatternGroup == spawnPatternsEasy && chosenPattern == 0){
			chosenPattern = Random.Range(2, 3);
		}

		activePattern = chosenPatternGroup[chosenPattern];

		patternIndex = 0;

		gc.SetSegmentColors(activeColors);

		spawnCount = 16;
	}

	private void SpawnOrb(){
		GameObject newOrb = (GameObject)Instantiate(orbPrefab, transform.position, Quaternion.identity);
		OrbScript newOrbScript = newOrb.GetComponent<OrbScript>();
		newOrbScript.SetGC(gc);

		//Set direction of orb
		float angle = (Random.Range(0, 8)*45) + 22.5f;
		newOrbScript.SetMovementParams(angle, orbSpeed);

		int col = GetColorForOrb();

		newOrbScript.SetColor(col, gc.getMaterial(col));

		//gc.SFXSpawn();

		spawnCount--;
		if(spawnCount == 0){
			gc.DoneSpawning(newOrbScript);
		}
	}

	public void SetSpawnRate(float rate){
		//spawnRate = rate;
	}

	public void SetOrbSpeed(float spd){
		orbSpeed = spd;
	}
		
	public int GetColorForOrb(){
		return activeColors[Random.Range(0, activeColors.Length)];
	}

	private int[] CreateRandomColorSetup(int c, int a){
		//Create randomized array consisting of all numbers in range [0..a]
		int[] randomOrder = new int[a];
		for(int i = 0; i < a; i++){
			randomOrder[i] = i;
		}
		for (int i = randomOrder.Length - 1; i > 0; i--) {
			var r = Random.Range(0,i);
			int tmp = randomOrder[i];
			randomOrder[i] = randomOrder[r];
			randomOrder[r] = tmp;
		}
		//Pick the first c numbers from this array
		int[] result = new int[c];

		for(int i = 0; i < c; i++){
			result[i] = randomOrder[i];
		}

		return result;
	}

	public void SetPaused(bool p){
		paused = p;
	}
}
