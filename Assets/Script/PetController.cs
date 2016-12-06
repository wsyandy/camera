using UnityEngine;
using System.Collections;
using LitJson;
using System.Collections.Generic;

public class PetController : MonoBehaviour {
	public GameObject pet;
	public UILabel debugLabel;
	public GameObject fingerGesture;
	public Vector3 target;
	public bool can_move;

	private string petName,platform,url;
	private List<Material> thisMaterial;
	private List<string> shaders;
	private float forwardSpeed=7.0f;
	private Animation anim;
	private List<string> animations=new List<string>();
	private UIController uictrl;
	private int userid;

	void Awake(){
//		Caching.CleanCache ();
		url = PlayerPrefs.GetString ("url");
		userid = PlayerPrefs.GetInt ("userid");
		StartCoroutine (GetAssistant ());

//		StartScript();
		#if UNITY_EDITOR
		Debug.Log("Unity Editor");
		#endif

		#if UNITY_IOS
		platform="ios_";
		Debug.Log("ios");
		#endif

		#if UNITY_STANDALONE_OSX
		Debug.Log("Stand Alone OSX");
		#endif

		#if UNITY_STANDALONE_WIN
		Debug.Log("Stand Alone Windows");
		#endif
	}

	// Use this for initialization
	void Start () {
		target = Vector3.zero;
		uictrl = GameObject.Find ("UI Root").GetComponent<UIController> ();

		thisMaterial = new List<Material>(6);  
		shaders = new List<string>(6);  

		MeshRenderer[] meshRenderer = GetComponentsInChildren<MeshRenderer>();  
		int length = meshRenderer.Length;  

		for (int i = 0; i < length; i++) {  
			int count = meshRenderer [i].materials.Length;  
			for (int j = 0; j < count; j++) {  
				Material _mater = meshRenderer [i].materials [j];  
				thisMaterial.Add (_mater);  
				shaders.Add (_mater.shader.name);  
			}  
		}
		SkinnedMeshRenderer[] meshSkinRenderer = GetComponentsInChildren<SkinnedMeshRenderer>();  
		length = meshSkinRenderer.Length;  

		for (int i = 0; i < length; i++)  
		{  
			int count = meshSkinRenderer[i].materials.Length;  
			for (int j = 0; j < count; j++)  
			{  
				Material _mater = meshSkinRenderer[i].materials[j];  
				thisMaterial.Add(_mater);  
				shaders.Add(_mater.shader.name);  
			}  
		}  


		for( int i = 0; i < thisMaterial.Count; i++)  
		{  
			thisMaterial[i].shader = Shader.Find(shaders[i]);  
		}  
	}
	
	// Update is called once per frame
	void Update () {
		if (pet != null) {
			float distance = Vector3.Distance (target, pet.transform.position);
			if (distance > 0.3f) {
				pet.transform.LookAt (target);
				pet.transform.Translate (Vector3.forward * forwardSpeed * Time.deltaTime);
			}
		
			if (!anim.isPlaying)
				anim.Play ();
		}
	}

	public void SetGesture(MyGestureParameter mgp){
		switch (mgp.gesturename) {
		case "tap":
			target = mgp.v3;
			break;
		case "swipe":
			pet.transform.Rotate (0, mgp.angle, 0);
			break;
		case "longpress":
			pet.SetActive (mgp.isShow);
			break;
		}
	}

	public void ResetClick(){
		can_move = false;
		int index = Random.Range (0, animations.Count - 1);
		anim.CrossFade (animations[index]);
	}

	IEnumerator LoadPet(string path){
		WWW www = WWW.LoadFromCacheOrDownload (path, 0);
		yield return www;
		AssetBundle bundle = www.assetBundle;
		AssetBundleRequest request = bundle.LoadAssetAsync (petName, typeof(GameObject));
		yield return request;
		GameObject temp = request.asset as GameObject;
		pet = Instantiate (temp);
		StartScript ();
		anim=pet.GetComponent<Animation> ();
		foreach (AnimationState state in anim) {
			animations.Add (state.name);
		}
		bundle.Unload (false);
		www.Dispose ();
	}

	IEnumerator GetAssistant(){
		WWWForm form = new WWWForm ();
		form.AddField ("userid", userid);
		WWW w = new WWW (url+"getassistant", form);
		yield return w;
		if (w.error != null) {
			debugLabel.text = w.error;
			print (w.error);
		} else {
			print (w.text);
			JsonData data = JsonMapper.ToObject (w.text);
			petName = (string)data ["name"];
			string path = (string)data ["url"]+platform+petName;
			w.Dispose ();
			yield return StartCoroutine (LoadPet (path));
			yield return StartCoroutine (SendCard ());
			StartCoroutine (ReceiveCard ());
		}
	}

	private void StartScript(){
		fingerGesture.SetActive (true);
	}

	IEnumerator SendCard(){
		int send = PlayerPrefs.GetInt ("send",0);
		if (send == 1) {
			uictrl.dialog.gameObject.SetActive (true);
			uictrl.dialogLabel.text = "主人，我去去就回";
			target = new Vector3 (0, 0, 200);
			yield return new WaitForSeconds (3);
			target = Vector3.zero;
			yield return new WaitForSeconds (3);
			uictrl.dialogLabel.text = "卡牌已经送达";
			yield return new WaitForSeconds (1);
			uictrl.dialog.gameObject.SetActive (false);
			send = 0;
		}
	}

	IEnumerator ReceiveCard(){
		WWWForm form = new WWWForm ();
		form.AddField ("userid", userid);
		WWW w = new WWW (url+"getreceivedequipmentlist", form);
		yield return w;
		if (w.error != null) {
			debugLabel.text = w.error;
			print (w.error);
		} else {
			print (w.text);
			if (w.text != "None") {
				string name = "";
				JsonData data = JsonMapper.ToObject (w.text);
				for (int i = 0; i < data.Count; i++) {
					string temp = (string)data [i] ["srcusername"];
					if (!name.Contains (temp)) {
						name += (string)data [i] ["srcusername"] + " ";
					}
				}
				uictrl.dialog.gameObject.SetActive (true);
				uictrl.dialogLabel.text=name+"赠送给您卡牌了";
				target = new Vector3 (0, 0, 200);
				yield return new WaitForSeconds (3);
				target = Vector3.zero;
				yield return new WaitForSeconds (3);
				uictrl.dialogLabel.text = "卡牌已经送达";
				yield return new WaitForSeconds (1);
				uictrl.dialog.gameObject.SetActive (false);
			}
		}
	}
}
