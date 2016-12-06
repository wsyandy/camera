using UnityEngine;
using System.Collections;

public class Pet : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider collider)
	{
		UIController uictrl = GameObject.Find ("UI Root").GetComponent<UIController> ();
		uictrl.LogoClick (2, collider.name);
		collider.gameObject.SetActive (false);
	}
}
