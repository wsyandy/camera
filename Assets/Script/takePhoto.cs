using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime .Serialization.Formatters.Binary;
using System.Threading;
using UnityEngine.UI;
using LitJson;

public class takePhoto : MonoBehaviour 
{
	public string gps_info="";
	public UILabel debugLabel;
	public float x=0f,y = 0f;//经纬度
	public TextMesh text3d;
	public GameObject chair;
	public float SCREEN_SHOT_WIDTH = 1000;
	public Camera myCamera;
	public UILabel cardLabel;//卡牌显示
	/// <summary>
	/// 连续拍照的次数
	/// </summary>
	public int continue_num = 1;

	private string json="";
	private Vector2 position;//点击屏幕的点

	/// <summary>
	/// 获取截图
	/// </summary>
	/// <returns>The texture.</returns>
	IEnumerator getTexture(float startXPosition,float startYPosition,int width,int height,string url="http://159.226.21.146:3306/objectdetect")
	{
		while (this.continue_num > 0) {
			this.continue_num--;
			yield return new WaitForEndOfFrame ();
			RenderTexture rt = new RenderTexture (width, height, 24);
			myCamera.targetTexture = rt;
			myCamera.Render ();
			yield return new WaitForEndOfFrame ();
			RenderTexture.active = rt;
			Texture2D t = new Texture2D (width, height, TextureFormat.RGB24, false);
			t.ReadPixels (new Rect (startXPosition, startYPosition, width, height), 0, 0);
			t.Apply ();
			myCamera.targetTexture = null;
			RenderTexture.active = null;
			GameObject.Destroy (rt);
			yield return new WaitForEndOfFrame ();
			byte[] byt = t.EncodeToPNG ();
//		File.WriteAllBytes(Application.dataPath+"/Photoes/"+Time.time+".jpg",byt);
			StartCoroutine (UploadImg (byt, url));
			yield return new WaitForSeconds (1);
		}
	}

	/// <summary>
	/// gps click
	/// </summary>
	public void getGPS(){
		StartCoroutine (StartGPS ());
		if (this.x == 0f) {
			this.x = 36.2662f;
			this.y = 120.272f;
		}
		string url = "http://api.map.baidu.com/place/v2/search?query=美食&location=" + this.x + "," + this.y + "&radius=2000&output=json&ak=ABMyPFHzCuKItIEoAG2FZjtt";
		StartCoroutine (GetJsonAndShowCard (url));
	}

	/// <summary>
	/// Gets the json and show card.
	/// </summary>
	/// <returns>The json and show card.</returns>
	/// <param name="url">URL.</param>
	IEnumerator GetJsonAndShowCard(string url){
		WWW getdata = new WWW (url);
		yield return getdata;
		if (getdata.error != null) {
			debugLabel.text = getdata.error;
		} else {
			this.json = getdata.text;
		}
		Debug.Log (this.json);
		JsonData data=JsonMapper.ToObject(this.json);
		if((int)data["status"]==0){
			Debug.Log(data["results"][0]["name"]);
			cardLabel.text = (string)data ["results"] [0] ["name"];
		}
	}

	/// <summary>
	/// Starts the GP.
	/// </summary>
	/// <returns>The GP.</returns>
	IEnumerator StartGPS(){
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
			yield break;
		} else {
			debugLabel.text = "纬度" + Input.location.lastData.latitude + "\r\n经度" + Input.location.lastData.longitude;
			this.x = Input.location.lastData.latitude;
			this.y = Input.location.lastData.longitude;
		}
	}

	/// <summary>
	/// Mies the take photo.
	/// </summary>
	/// <param name="position">Position.</param>
	public void MyTakePhoto(Vector2 position){
//		Debug.Log (position);
		this.continue_num = 1;
		this.position = position;
		StartCoroutine (getTexture (position.x - 500, position.y - 500, 1000, 1000));
	}

	/// <summary>
	/// 检测手势
	/// </summary>
	public void GetHandGesture(){
		if (this.continue_num < 2) {
			this.continue_num = 10000;
			float tempScale = SCREEN_SHOT_WIDTH / Screen.width;
			StartCoroutine (getTexture (0, 0, (int)(Screen.width * tempScale), (int)(Screen.height * tempScale), "http://159.226.21.146:3306/handrecog"));
		} else {
			this.continue_num = 1;
		}
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
		yield return getData;
		if (getData.error != null) {
			Debug.Log (getData.error);
		} else {
			this.json = getData.text;
			debugLabel.text = getData.text;
			Debug.Log (getData.text);
		}
	}
}