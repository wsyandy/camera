using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Gestures : MonoBehaviour {
	public Image kuang;
	// Use this for initialization
	void Start () {
		LongPressRecognizer longPress = GetComponent<LongPressRecognizer>();
		longPress.OnGesture += MyLongPressEventHandler;
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
//		Debug.Log ("longpress" + gesture.Position);
		kuang.gameObject.SetActive(true);
		kuang.transform.position = gesture.Position;
		GameObject.Find("CameraController").GetComponent<takePhoto>().MyTakePhoto(gesture.Position);
		StartCoroutine (DisableKuang ());
	}

	/// <summary>
	/// Disables the kuang.
	/// </summary>
	/// <returns>The kuang.</returns>
	IEnumerator DisableKuang(){
		yield return new WaitForSeconds (3);
		kuang.gameObject.SetActive (false);
	}
}
