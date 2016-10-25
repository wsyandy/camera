using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Login : MonoBehaviour {

	public UILabel nameLabel;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnClick(){
//		string name=nameLabel.text;
//		Debug.Log(name);
		SceneManager.LoadScene("camera");
	}
}
