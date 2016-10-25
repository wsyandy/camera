using UnityEngine;
using System.Collections;

public class MyRotate : MonoBehaviour {

	// Use this for initialization
	void Start () {
		SwipeRecognizer swipe = GetComponent<SwipeRecognizer> ();
		swipe.OnGesture += Swipe_Gesture;
	}
	
	// Update is called once per frame
	void Update () {
	}

	void Swipe_Gesture(SwipeGesture e){
		Vector2 move = e.Move;
		float velocity = e.Velocity;
		FingerGestures.SwipeDirection direction = e.Direction;
		int angle = direction.ToString () == "Left" ? 45 : -45;
		transform.Rotate (new Vector3 (transform.position.x, transform.position.y + angle, transform.position.y));
		Debug.Log("move="+move.ToString()+"速度"+velocity+"方向"+direction.ToString());
	}
}
