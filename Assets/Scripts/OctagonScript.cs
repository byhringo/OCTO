using UnityEngine;
using System.Collections;

public class OctagonScript : MonoBehaviour {

	public GameController gc;

	public SegmentScript[] segments;
	public SegmentOverlayScript[] segmentOverlays;
	public BGSegmentScript[] bgSegments;

	private float angle = 0;
	public float spinSpeed;

	/* 0 - Not initialized, not accepting input
	 * 1 - Initialized and playing, accepting input
	 * 2 - First half of levelup-animation, accepting input
	 * 3 - Second half of levelup-animation, accepting input
	 * 4 - Dead, not accepting input
	 */
	private int state = 0;

	private bool paused = false;

	private float animTime;
	private float animVal = 0f;

	// Use this for initialization
	void Start () {
		animTime = (GameController.bpm/300f);

		//Bad solution? works!
		foreach(BGSegmentScript bgs in bgSegments){
			bgs.SetGC(gc);
		}
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if((state == 1 || state == 2 || state == 3) && !paused){
			bool leftDown = false;
			bool rightDown = false;

			//Touch input
			//TODO: This
			for(int i = 0; i < Input.touchCount; i++){
				if(Input.GetTouch(i).position.x < Screen.width*0.4f){
					leftDown = true;
				}
				if(Input.GetTouch(i).position.x > Screen.width*0.6f){
					rightDown = true;
				}
			}


			//PC input
			if(Input.GetKey(KeyCode.A)){
				leftDown = true;
			}
			if(Input.GetKey(KeyCode.D)){
				rightDown = true;
			}

			if(leftDown && !rightDown){
				angle -= Time.fixedDeltaTime*spinSpeed;
				if(angle < 0){
					angle += 360;
				}
			}
			else if(!leftDown && rightDown){
				angle += Time.fixedDeltaTime*spinSpeed;
				if(angle >= 360){
					angle -= 360;
				}
			}
			else{
				//angle = (Mathf.PI/4f) * (float) Mathf.Round(angle / (Mathf.PI/4f));
				angle = GetRoundedAngle(angle);
				if(angle < 0){
					angle += 360;
				}
				else if(angle >= 360){
					angle -= 360;
				}
			}
		}
		if(state == 2 && !paused){
			animVal = Mathf.Min(animVal + Time.fixedDeltaTime, animTime);

			Vector3 newScale = new Vector3(1 - BackEaseIn(animVal, 0f, 1f, animTime), 1, 1);

			for(int i = 0; i < segments.Length; i++){
				segments[i].transform.localScale = newScale;
			}

			if(animVal == animTime){
				SetState(3);
				animVal = 0;
				gc.AnimationStage1Complete();
			}
		}
		else if(state == 3 && !paused){
			animVal = Mathf.Min(animVal + Time.fixedDeltaTime, animTime);

			Vector3 newScale = new Vector3(BackEaseOut(animVal, 0f, 1f, animTime), 1, 1);

			for(int i = 0; i < segments.Length; i++){
				segments[i].transform.localScale = newScale;
			}

			if(animVal == animTime){
				gc.AnimationStage2Complete();
			}
		}

		transform.rotation = Quaternion.identity;
		transform.RotateAround(Vector3.zero, Vector3.up, GetRoundedAngle(angle));
	}

	public static float BackEaseIn(float t, float b, float c, float d){
		return c * ( t /= d ) * t * ( ( 8f + 1 ) * t - 8f ) + b;
	}

	public static float BackEaseOut(float t, float b, float c, float d)	{
		return c * ( ( t = t / d - 1 ) * t * ( ( 4 + 1 ) * t + 4 ) + 1 ) + b;
	}

	public void SetSegmentColors(int[] activeColors){
		int[] pattern = CreatePattern(activeColors.Length);

		//Cross reference the pattern and the active colors, then set the segment colors correctly
		for(int i = 0; i < 8; i++){
			int c = pattern[i];

			if(c != -1){
				segments[i].SetColor(activeColors[c], gc.getMaterial(activeColors[c]));
			}
			else{
				segments[i].SetColor(c, gc.getMaterial(c));
			}
		}
	}

	//Patterns for the 8 different color counts
	int[][] pattern1 = {
		new int[] {0, 0, -1, -1, 0, 0, -1, -1},
		new int[] {0, 0, -1, 0, 0, -1, -1, -1},
		new int[] {0, 0, -1, -1, 0, 0, -1, -1},
	};
	
	int[][] pattern2 = {
		new int[] {0, 0, 0, -1, 1, 1, -1, -1},
		new int[] {0, 0, 1, 1, 0, 0, -1, -1},
		new int[] {0, 0, 0, -1, 1, 1, 1, -1},
		new int[] {0, 0, -1, 1, 1, -1, -1, -1},
	};
		
	int[][] pattern3 = {
		new int[] {0, 0, 1, 1, 2, 2, -1, -1},
		new int[] {0, 0, -1, 1, 1, -1, 2, 2},
	};
		
	int[][] pattern4 = {
		new int[] {0, 0, 1, 1, 2, 2, 3, 3},
		new int[] {0, 1, 0, 1, 2, 3, 2, 3},
		new int[] {0, 1, 2, 3, -1, -1, -1, -1},
		new int[] {0, 1, 2, -1, -1, 3, -1, -1},
	};
		
	int[][] pattern5 = {
		new int[] {0, 0, 1, 2, 2, 3, 4, 4},
		new int[] {0, -1, -1, 0, 1, 2, 3, 4},
		new int[] {0, -1, 1, 2, 3, 4, -1, 0},
	};

	int[][] pattern6 = {
		new int[] {0, 0, 1, 2, 3, 3, 4, 5},
		new int[] {0, 1, 2, -1, 3, 4, 5, -1},
	};
		
	int[][] pattern7 = {
		new int[] {0, 1, 2, 3, 4, 5, 6, -1},
	};
		
	int[][] pattern8 = {
		new int[] {0, 1, 2, 3, 4, 5, 6, 7},
	};

	//Creates a pattern for the octagon based on current level and number of active colors
	private int[] CreatePattern(int c){
		switch (c){
		case 1: return pattern1[Random.Range(0, pattern1.Length)];
		case 2: return pattern2[Random.Range(0, pattern2.Length)];
		case 3: return pattern3[Random.Range(0, pattern3.Length)];
		case 4: return pattern4[Random.Range(0, pattern4.Length)];
		case 5: return pattern5[Random.Range(0, pattern5.Length)];
		case 6: return pattern6[Random.Range(0, pattern6.Length)];
		case 7: return pattern7[Random.Range(0, pattern7.Length)];
		case 8: return pattern8[Random.Range(0, pattern8.Length)];
		default: return new int[8];
		}
	}

	public void SetGC(GameController gameControl){
		gc = gameControl;
	}

	private float GetRoundedAngle(float angle){
		return (45f) * (float) Mathf.Round(angle / (45f));
	}

	public int GetColorForSegment(int id){
		return segments[id].GetColor();
	}

	public float GetRotation(){
		return angle;
	}

	public void ShineOverlay(int i){
		segmentOverlays[i].Shine();

		//The octagon is rotated, but the background segments are not.
		int segmentIndex = (i + Mathf.FloorToInt(GetRoundedAngle(angle)/45f)) % 8;

		bgSegments[segmentIndex].SetColor(segments[i].GetColor());
	}

	public void SetState(int newState){
		state = newState;
		animVal = 0;
	}

	public void SetPaused(bool p){
		paused = p;
	}
}
