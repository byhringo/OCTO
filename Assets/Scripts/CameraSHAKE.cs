using UnityEngine;
using System.Collections;

public class CameraSHAKE : MonoBehaviour {

	public GameController gc;

	private Vector3 initPos;
	private Quaternion initRot;

	private float shakeDuration = 0;
	private float shakeIntensity = 0;
	private float rotation = 0;
	private float spinSpeed = 0;
	private float currentSpinSpeed = 0; //This smooths the changing of spin direction
	private float spinTimer = 0f; //How long until we change direction and speed next time?

	// Use this for initialization
	void Start () {
		initPos = transform.position;
		initRot = transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {
		if(spinSpeed != 0 && !gc.IsPaused()){
			spinTimer -= Time.deltaTime;

			if(spinTimer <= 0){
				ChangeSpin();
			}
		}

		transform.position = initPos;
		transform.rotation = initRot;
		transform.RotateAround(Vector3.zero, Vector3.up, rotation);

		rotation += Time.deltaTime*currentSpinSpeed;

		currentSpinSpeed = Mathf.Lerp(currentSpinSpeed, spinSpeed, 3f*Time.deltaTime);

		shakeDuration -= Time.deltaTime;

		if(shakeDuration >= 0f){
			float shakeVal = shakeDuration * shakeIntensity;

			Vector3 shakeVec = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
			shakeVec.Normalize();

			transform.position += shakeVec*shakeVal;
		}
	}

	public void ShakeItUp(float d, float i){
		shakeDuration = d;
		shakeIntensity = i;
	}

	public void StartSpinning(){
		if(spinSpeed == 0){
			ChangeSpin();
		}
	}

	private void ChangeSpin(){
		int sl = gc.GetSpinLevel();
		float newMinSpinSpeed = sl*6f;
		float newMaxSpinSpeed = sl*8f;

		spinSpeed = Mathf.Min(Random.Range(newMinSpinSpeed, newMaxSpinSpeed), 75f) * (spinSpeed > 0 ? -1 : 1);

		//Set a new random timer for when we should change the spin the next time
		int addedSeconds = Mathf.Min(0, 8 - sl);

		spinTimer = Random.Range(6f + addedSeconds, 10f + addedSeconds);
	}

	public void ResetRotation(){
		transform.rotation = initRot;
		rotation = 0;
		spinSpeed = 0;
	}
}
