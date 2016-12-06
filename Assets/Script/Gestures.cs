using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Gestures : MonoBehaviour {
	public Vector3 target;
	public  Vector2 tapPoint;

	private PetController pctrl;
	private takePhoto tp;
	private UIController uictrl;

	// Use this for initialization
	void Start () {
		LongPressRecognizer longPress = GetComponent<LongPressRecognizer>();
		longPress.OnGesture += MyLongPressEventHandler;
		TapRecognizer tapRecognizer = GetComponent<TapRecognizer> ();
		tapRecognizer.OnGesture += Tap_Gesture;
		SwipeRecognizer swipe = GetComponent<SwipeRecognizer> ();
		swipe.OnGesture += Swipe_Gesture;
		pctrl = GameObject.Find ("GameController").GetComponent<PetController> ();
		tp = GameObject.Find ("CameraController").GetComponent<takePhoto> ();
		uictrl = GameObject.Find ("UI Root").GetComponent<UIController> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	/// <summary>
	/// Mies the long press event handler.
	/// </summary>
	/// <param name="gesture">Gesture.</param>
	void MyLongPressEventHandler( LongPressGesture gesture )
	{
		
		MyGestureParameter mgp = new MyGestureParameter ("longpress");
		pctrl.SetGesture (mgp);
		Vector2 v2 = new Vector2 (gesture.Position.x - Screen.width  / 2, gesture.Position.y - Screen.height  / 2);
		uictrl.SetSpriteStatus ("kuang", true, v2);
		tp.MyTakePhoto (gesture.Position);
		StartCoroutine (DisableKuang ());

	}

	/// <summary>
	/// Disables the kuang.
	/// </summary>
	/// <returns>The kuang.</returns>
	IEnumerator DisableKuang(){
		yield return new WaitForSeconds (3);
		uictrl.SetSpriteStatus ("kuang", false);
		MyGestureParameter mgp = new MyGestureParameter ("longpress");
		mgp.isShow = true;
		pctrl.SetGesture (mgp);
	}

	void Tap_Gesture(TapGesture e){
		tapPoint = new Vector2 (e.Position.x - Screen.width / 2, e.Position.y - Screen.height / 2);
		GameObject hittedObject = e.Selection;
		var tempObject = UICamera.hoveredObject;
		bool isNoClickUI = true;
		if (tempObject != null && tempObject.name != "UI Root") {
			isNoClickUI = false;
		}
		if (null != hittedObject && isNoClickUI) {
			if ("Plane" == hittedObject.name) {
				pctrl.can_move = true;
				target = e.Raycast.Hit3D.point;
				MyGestureParameter mgp = new MyGestureParameter ("tap");
				mgp.v3 = target;
				pctrl.SetGesture (mgp);
			}
			Debug.Log (hittedObject.name + "pos:" + target);
		}
	}

	void Swipe_Gesture(SwipeGesture e){
		Vector2 move = e.Move;
		float velocity = e.Velocity;
		FingerGestures.SwipeDirection direction = e.Direction;
		int angle = direction.ToString () == "Left" ? 45 : -45;
		MyGestureParameter mgp = new MyGestureParameter ("swipe");
		mgp.angle = angle;
		pctrl.SetGesture (mgp);
	}
}
