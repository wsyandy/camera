using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using LitJson;

public class Login : MonoBehaviour {

	public UILabel nameLabel,debugLabel;
	public int arg1=0,arg2=1;

	void Start(){
		string name = PlayerPrefs.GetString ("name","");
		if(name!=""){
//			SceneManager.LoadScene("camera", LoadSceneMode.Single);
		}
	}

	public void OnClick(int position,string name){
		string ip = position == 0 ? "172.18.32.75" : "159.226.21.146";
		string url="http://"+ip+":3306/api/";
		PlayerPrefs.SetString ("url", url);
		StartCoroutine(_login(name,url+"login"));
	}

	private IEnumerator _login(string name,string url){
		WWWForm form = new WWWForm ();
		form.AddField ("name", name);
		WWW w = new WWW (url, form);
		yield return w;
		if (w.error != null) {
			debugLabel.text = w.error;
			print (w.error);
		} else {
			print (w.text);
			JsonData data=JsonMapper.ToObject(w.text);
			PlayerPrefs.SetInt ("userid", (int)data ["userid"]);
			PlayerPrefs.SetString ("image", (string)data ["image"]);
			PlayerPrefs.SetString ("name", (string)data["name"]);
			w.Dispose ();
			SceneManager.LoadScene("camera", LoadSceneMode.Single);
		}
	}

}
