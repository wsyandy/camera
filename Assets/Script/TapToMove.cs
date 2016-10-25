using UnityEngine;
using System.Collections;

public class TapToMove : MonoBehaviour {
	public float speed = 1.5f;
	Vector3 target;
	private Animator anim;
	private Vector3 velocity;

	public float forwardSpeed=7.0f;

	// Use this for initialization
	void Start () {
		TapRecognizer tapRecognizer = GetComponent<TapRecognizer> ();
		tapRecognizer.OnGesture += Tap_Gesture;
		anim = GetComponent<Animator> ();
		target = new Vector3 (0, 0, 0);
	}
	
	// Update is called once per frame
	void Update () {
		float distance = Vector3.Distance(target, transform.position);
		if (distance > 0.3f) {
			float v = 1.0f;
			anim.SetFloat ("Speed", v);
			velocity = new Vector3 (0, 0, v);
			velocity = transform.TransformDirection (velocity);
			velocity *= forwardSpeed;
			transform.localPosition += velocity * Time.deltaTime;

//			transform.Translate (target * Time.deltaTime);
		} else {
			
		}
	}

	void Tap_Gesture(TapGesture e){
		GameObject hittedObject = e.Selection;
		if (null != hittedObject) {
			if ("Plane" == hittedObject.name) {
				target = e.Raycast.Hit3D.point;
				transform.LookAt (target);
			}
			Debug.Log (hittedObject.name + "pos:" + target);
		} else {
			Debug.Log ("error");
		}
	}

}
