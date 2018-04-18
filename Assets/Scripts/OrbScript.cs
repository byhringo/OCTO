using UnityEngine;
using System.Collections;

public class OrbScript : MonoBehaviour {
	
	private GameController gc;
	private int currentColor;

	private float directionAngle = 0;
	private float speed = 0;
	private float dist = 0;
	private float scaleSpeed = 20f;
	private bool destroyed = false;
	private bool paused = false;

	private bool initialized = false;

	private float targetScale = 1.3f;

	// Use this for initialization
	void Start () {
	}

	// Update is called once per frame
	void FixedUpdate () {
		if(initialized && !destroyed && !paused){
			if(transform.localScale.x < targetScale){
				transform.localScale += Vector3.one*targetScale*Time.fixedDeltaTime*scaleSpeed;

				if(transform.localScale.x > targetScale){
					transform.localScale = Vector3.one*targetScale;
				}
			}

			dist += Time.fixedDeltaTime*speed;

			if(dist >= gc.GetCollisionDistance()){
				gc.RegisterOrbCollision(this);
			}

			transform.position = Vector3.forward * dist;
			transform.RotateAround(Vector3.zero, Vector3.up, directionAngle);
			transform.localRotation = Quaternion.identity;
		}
		else if (destroyed && !paused){
			transform.localScale -= Vector3.one*targetScale*Time.fixedDeltaTime*scaleSpeed;
			if(transform.localScale.x <= 0){
				Destroy(gameObject);
			}
		}
	}

	public void SetGC(GameController gameControl){
		gc = gameControl;
	}

	public void SetMovementParams(float dirAngle, float spd){
		directionAngle = dirAngle;
		speed = spd;
		transform.localScale = Vector3.zero;
		initialized = true;
	}

	public void SetColor(int colorID, Material m){
		GetComponent<Renderer>().material = m;
		currentColor = colorID;
	}

	public int GetColor(){
		return currentColor;
	}

	public float GetAngle(){
		return directionAngle;
	}

	public void GoAway(){
		destroyed = true;
	}

	public void SetPaused(bool p){
		paused = p;
	}
}