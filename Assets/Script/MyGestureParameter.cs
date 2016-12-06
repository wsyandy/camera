using UnityEngine;

public class MyGestureParameter {
	public string gesturename;
	public Vector3 v3;
	public int angle;
	public bool isShow;

	public MyGestureParameter(string name){
		gesturename = name;
		v3 = new Vector3 ();
		angle = 0;
		isShow = false;
	}

}
