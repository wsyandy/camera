using UnityEngine;
using System.Collections;

public class TestController : MonoBehaviour {
	public UIScrollBar bar;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void left(){
		bar.GetComponent<UIScrollBar> ().value -= 0.001f;
	}

	public void right(){
		bar.GetComponent<UIScrollBar> ().value += 0.001f;
	}
}
