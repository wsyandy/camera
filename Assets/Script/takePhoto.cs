using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime .Serialization.Formatters.Binary;
using System.Threading;
using UnityEngine.UI;
using LitJson;
using UnityEngine.SceneManagement;

public class takePhoto : MonoBehaviour 
{
	public UILabel debugLabel;
	public Camera myCamera;
	/// <summary>
	/// The recognize model.0:物体识别，1：logo识别，2：手势识别
	/// </summary>
	public int recognizeModel;
	/// <summary>
	/// The card v3.
	/// </summary>
	public Vector2 card_v2;
	/// <summary>
	/// 是否获取手势
	/// </summary>
	public bool isgetGesture;
	public string ip;

	private string _recognizerUrl,_gestureUrl,_logoUrl,_messageUrl;
	/// <summary>
	/// 截图的矩形区域
	/// </summary>
	public Rect _rect;
	public GameObject people;

	private string json="";
	/// <summary>
	/// 提示音
	/// </summary>
	private AudioSource _audioSource;
	/// <summary>
	/// 限制update中执行的频率
	/// </summary>
	private int time;
	/// <summary>
	/// 目的地图片
	/// </summary>
	public UISprite destationSprite;
	/// <summary>
	/// The logo card.
	/// </summary>
	public UISprite _logoCard;

	private float SCREEN_SHOT_WIDTH=1000;
	private float x=0, y=0;
	private UIController uictrl;
	private PetController pctrl;
	private string url,platform;
	private GameObject good;

	void Start(){
		card_v2 = new Vector2 (300, 0);
		this._audioSource = GameObject.Find ("CameraController").GetComponent<AudioSource> ();
		url = PlayerPrefs.GetString ("url");
		_recognizerUrl = url + "objectdetect";
		_gestureUrl=url + "handrecog";
		_logoUrl = url + "imginfo";
		_messageUrl = url + "getcommoninterests";
		uictrl = GameObject.Find ("UI Root").GetComponent<UIController> ();
		pctrl = GameObject.Find ("GameController").GetComponent<PetController> ();
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

	/// <summary>
	/// gps click
	/// </summary>
	public void GetGPS(){
		pctrl.can_move = false;
		recognizeModel = 2;
		pctrl.pet.SetActive (false);
		StartCoroutine (StartGPS ());
	}

	/// <summary>
	/// Mies the take photo.
	/// </summary>
	/// <param name="position">Position.</param>
	public void MyTakePhoto(Vector2 position){
		string url = recognizeModel == 0 ? _recognizerUrl : _logoUrl;
		StartCoroutine (getTexture (new Rect(position.x - 500, position.y - 500, 1000, 1000),url));
	}
		
	/// <summary>
	/// 获取截图
	/// </summary>
	/// <returns>The texture.</returns>
	/// <param name="rect">截图的矩形</param>
	/// <param name="url">传输的url</param>
	IEnumerator getTexture(Rect rect,string url){
		yield return new WaitForEndOfFrame ();
		RenderTexture rt = new RenderTexture ((int)rect.width, (int)rect.height,24);
		myCamera.targetTexture = rt;
		myCamera.Render ();
		RenderTexture.active = rt;
		Texture2D t = new Texture2D ((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
		t.ReadPixels (rect, 0, 0);
		t.Apply ();
		myCamera.targetTexture = null;
		RenderTexture.active = null;
		GameObject.Destroy (rt);
		byte[] byt = t.EncodeToPNG ();
		if (Application.isEditor) {
			File.WriteAllBytes (Application.dataPath + "/Photoes/" + Time.time + ".jpg", byt);
		}
		StartCoroutine (UploadImg (byt, url));
		GameObject.Destroy(t);
		byt = null;
	}

	/// <summary>
	/// 上传图片
	/// </summary>
	/// <returns>The image.</returns>
	/// <param name="byt">图片字节流</param>
	/// <param name="url">上传地址</param>
	IEnumerator UploadImg(byte[] byt,string url){
		WWWForm form = new WWWForm ();
		form.AddBinaryData ("photo", byt);
		WWW getData = new WWW (url, form);
		form = null;
		yield return getData;
		if (getData.error != null) {
			Debug.Log (getData.error);
		} else {
			this.json = getData.text;
			Debug.Log (getData.text);
			if (this.json != "None") {
				JsonData data = JsonMapper.ToObject (this.json);
				switch (this.recognizeModel) {
				case 2:
					int position_x = (int)data ["x"];
					int position_y = (int)data ["y"];
					float tempScale = SCREEN_SHOT_WIDTH / Screen.width;
					card_v2 = new Vector2 (position_x/tempScale-Screen.width/2, Screen.height/2 - position_y/tempScale);
					break;
				case 0:
					IDictionary dict = data as IDictionary;
					foreach (var item in dict.Keys) {
						Debug.Log (item);
						debugLabel.text = item.ToString();
						if (item.ToString () == "tvmonitor") {
							StartCoroutine (LoadGood ("tvmonitor"));
						}
					}
					break;
				case 1:
					string name = (string)data ["name"];
					uictrl.logoCard.gameObject.SetActive (true);
					uictrl.logoCard.spriteName = name;
					break;
				default:
					break;
				}
			}
			getData = null;
		}
	}

	public void Update(){
		if (isgetGesture) {
			if (time % 2 == 0) {
				if (time == 150) {
					StopAllCoroutines ();
					time = 0;
					System.GC.Collect ();
				}
				float distance = Vector2.Distance (default(Vector2), card_v2);
				Debug.Log (card_v2 + " " + distance);
				if (distance > 100f) {
					uictrl.card.transform.localPosition = card_v2;
					StartCoroutine (getTexture (this._rect, this._gestureUrl));
				} else {
					isgetGesture = false;
					uictrl.card.gameObject.SetActive (false);
					this._audioSource.Play ();
					uictrl.SetSpriteStatus ("destination", false);
					pctrl.pet.SetActive (true);
					uictrl.LogoClick (uictrl.gps, "美食卡牌");
				}
			}
			time++;
		}
	}

	/// <summary>
	/// Changes the recognizer model.
	/// </summary>
	public void ChangeRecognizerModel(){
		pctrl.can_move = false;
		recognizeModel = recognizeModel == 1 ? 0 : 1;
		string s = recognizeModel == 0 ? "当前为物体识别" : "当前为logo识别";
		this.debugLabel.text = s;
	}
		
	/// <summary>
	/// Lock this instance.
	/// </summary>
	public void Lock(){
		people.SetActive (!people.activeSelf);
	}

	public void OpenBag(){
		SceneManager.LoadScene ("friendlist", LoadSceneMode.Single);
	}
		
	/// <summary>
	/// Starts the GP.
	/// </summary>
	/// <returns>The GP.</returns>
	IEnumerator StartGPS(){
		yield return StartCoroutine (StartPhoneGps());
		if (x == 0f) {
			x = 36.2662f;
			y = 120.272f;
		}
		string url = "http://api.map.baidu.com/place/v2/search?query=" + WWW.EscapeURL ("美食") + "&location=" + this.x + "," + this.y + "&radius=2000&output=json&ak=ABMyPFHzCuKItIEoAG2FZjtt";
		WWW getdata = new WWW (url);
		yield return getdata;
		if (getdata.error != null) {
			debugLabel.text = getdata.error;
		} else {
			print (getdata.text);
			JsonData data = JsonMapper.ToObject (getdata.text);
			getdata.Dispose ();
			if ((int)data ["status"] == 0) {
				isgetGesture = true;
				float tempScale = SCREEN_SHOT_WIDTH / Screen.width;
				_rect = new Rect (0, 0, (int)(Screen.width * tempScale), (int)(Screen.height * tempScale));
				time = 0;
				uictrl.SetSpriteStatus ("card", true, new Vector2 (300, 0), (string)data ["results"] [0] ["name"]);
				uictrl.SetSpriteStatus ("destination", true);
			}
		}
		getdata.Dispose ();
	}

	IEnumerator StartPhoneGps(){
		//检查位置服务是否可用
		if (!Input.location.isEnabledByUser) {
			debugLabel.text = "位置服务不可用";
			yield break;
		}
		//开启位置服务
		debugLabel.text = "启动位置服务";
		Input.location.Start ();
		//等待服务初始化
		int maxWait = 20;
		while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0) {
			debugLabel.text = Input.location.status.ToString () + ">>>" + maxWait.ToString ();
			yield return new WaitForSeconds (1);
			maxWait--;
		}
		//服务初始化超时
		if (maxWait < 1) {
			debugLabel.text = "服务初始化超时";
			yield break;
		}
		//连接失败
		if (Input.location.status == LocationServiceStatus.Failed) {
			debugLabel.text = "无法确定设备位置";
		} else {
			debugLabel.text = "纬度" + Input.location.lastData.latitude + "\r\n经度" + Input.location.lastData.longitude;
			this.x = Input.location.lastData.latitude;
			this.y = Input.location.lastData.longitude;
		}
	}

	private IEnumerator LoadGood(string name){
		WWWForm form = new WWWForm ();
		form.AddField ("modelname", name);
		WWW w = new WWW (url+"getobjectmodel", form);
		yield return w;
		if (w.error != null) {
			print (w.error);
		} else {
			print (w.text);
			JsonData data = JsonMapper.ToObject (w.text);
			string downloadUrl = (string)data ["modelurl"];
			WWW www = WWW.LoadFromCacheOrDownload (downloadUrl+platform+name, 0);
			yield return www;
			AssetBundle bundle = www.assetBundle;
			AssetBundleRequest request = bundle.LoadAssetAsync (name, typeof(GameObject));
			yield return request;
			GameObject temp = request.asset as GameObject;
			good = Instantiate (temp,pctrl.target,Quaternion.identity) as GameObject;
			bundle.Unload (false);
			www.Dispose ();
		}
		w.Dispose ();
	}
}