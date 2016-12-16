using UnityEngine;
using System.Collections;
using LitJson;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ZoneGameController : MonoBehaviour {

	public UILabel debugLabel,buttonLabel,resultLabel;
	public UISprite ban;
	public GameObject resultText,contentListDialog,classDialog;

	private AudioSource myAudio;
	private GameObject speakButton, testButton, finshButton,contentGrid;
	private string url;
	private int id;
	private UIInput keywordInput;

	void Start()
	{
		speakButton = GameObject.Find ("Speak");
		testButton = GameObject.Find ("Test");
		finshButton = GameObject.Find ("Finish");
		contentGrid = GameObject.Find ("ContentGrid");
		keywordInput = GameObject.Find ("KeyWordInput").GetComponent<UIInput> ();

		string[] ms = Microphone.devices;
		int deviceCount = ms.Length;
		if (deviceCount == 0)
		{
			Log("no microphone found");
		}
		myAudio = GetComponent<AudioSource> ();
		finshButton.gameObject.SetActive (false);
		url = PlayerPrefs.GetString ("url");
		id=PlayerPrefs.GetInt("userid");

		StartCoroutine (LoadContent ());
	}
		
	public void SpeakClick(){
		if (buttonLabel.text == "开始录音") {
			buttonLabel.text = "停止录音";
			StartRecord ();
		} else {
			buttonLabel.text = "开始录音";
			Log ("正在分析");
			StopRecord ();
			StartCoroutine (UploadVoice ());
			contentListDialog.GetComponent<TweenScale> ().PlayForward ();
		}
	}

	/// <summary>
	/// 分词按钮点击事件
	/// </summary>
	public void BigBang(){
		ban.gameObject.SetActive (true);
		speakButton.gameObject.SetActive (false);
		testButton.gameObject.SetActive (false);
		StartCoroutine (Spilt (resultLabel.text));
	}

	/// <summary>
	/// 显示分词结果并关闭相应的按钮
	/// </summary>
	public void ShowBangItem(JsonData data){
		resultText.SetActive (false);
		ban.gameObject.SetActive (false);
		classDialog.SetActive (true);
		finshButton.gameObject.SetActive (true);
		GameObject itemGrid = GameObject.Find ("ItemGrid");
		itemGrid.transform.DestroyChildren ();
		for (int i = 0; i < data.Count; i++) {
			if ((string)data [i] ["pos"] != "wp") {
				GameObject gridItem = NGUITools.AddChild (itemGrid, (GameObject)(Resources.Load ("BangItemSprite")));
				gridItem.GetComponent<BangItem> ().label.text = (string)data [i] ["cont"];
			}
		}
		UIGrid ngui_ui_grid = itemGrid.GetComponent<UIGrid> ();
		ngui_ui_grid.repositionNow = true;
//		NGUITools.ImmediatelyCreateDrawCalls (
	}

	/// <summary>
	/// 完成按钮点击事件
	/// </summary>
	public void FinsihClick(){
		string s = resultLabel.text;
		string when = GetClassString (GameObject.Find ("When"));
		string where = GetClassString (GameObject.Find ("Where"));
		string who = GetClassString (GameObject.Find ("Who"));
		string _event = GetClassString (GameObject.Find ("Event"));
		string remark = GetClassString (GameObject.Find ("Remark"));
		StartCoroutine (UploadRecord (s, when, where, who,_event,remark));
	}

	public void BackClick(){
		SceneManager.LoadScene ("friendlist", LoadSceneMode.Single);
	}

	/// <summary>
	/// 搜索按钮点击事件
	/// </summary>
	public void OnSearchClick(){
		string keyword = keywordInput.value;
		StartCoroutine (LoadContent (keyword));
	}



	private void Log(string log)
	{
		debugLabel.text = log;
	}

	private void StartRecord()
	{
		myAudio.Stop();
		myAudio.loop = false;
		myAudio.mute = true;
		int maxRate=0, minRate=0;
		Microphone.GetDeviceCaps (null, out minRate, out maxRate);
		print (maxRate);
		myAudio.clip = Microphone.Start(null, true, 30, 8000);    
		while (!(Microphone.GetPosition(null) > 0))
		{
		}
		myAudio.Play();
		Log("StartRecord");
	}

	private void StopRecord()
	{
		if (!Microphone.IsRecording(null))
		{
			return;
		}
		Microphone.End(null);
		myAudio.Stop();
		Log ("");
	}

	void PrintRecord()
	{
		if (Microphone.IsRecording(null))
		{
			return;
		}
		byte[] data = GetClipData();
		string slog = "total length:" + data.Length + " time:" + myAudio.time;
		Log(slog);
	}

	public void PlayRecord()
	{
		if (Microphone.IsRecording(null))
		{
			print ("正在录音");
			return;
		}
		if (myAudio.clip == null)
		{
			print ("没有录音");
			return;
		}
		myAudio.mute = false;
		myAudio.loop = false;
		myAudio.Play();
	}

	//获取声音Byte数组数据
	public byte[] GetClipData()
	{
		if (myAudio.clip == null)
		{
			Debug.Log("GetClipData audio.clip is null");
			return null;
		}

		float[] samples = new float[myAudio.clip.samples];

		myAudio.clip.GetData(samples, 0);


		byte[] outData = new byte[samples.Length * 2];

		int rescaleFactor = 32767;

		for (int i = 0; i < samples.Length; i++)
		{
			short temshort = (short)(samples[i] * rescaleFactor);

			byte[] temdata = System.BitConverter.GetBytes(temshort);

			outData[i * 2] = temdata[0];
			outData[i * 2 + 1] = temdata[1];


		}
		if (outData == null || outData.Length <= 0)
		{
			Debug.Log("GetClipData intData is null");
			return null;
		}
		return outData;
	}

	private IEnumerator UploadVoice(){
		WWWForm form = new WWWForm ();
		form.AddBinaryData ("voice", GetClipData ());
		WWW w = new WWW (url + "voiceinfo", form);
		yield return w;
		if (w.error != null) {
			print (w.error);
		} else {
			print (w.text);
			if (w.text != "None") {
				JsonData data = JsonMapper.ToObject (w.text);
				string s = (string)data ["result"];
				resultText.SetActive (true);
				resultText.GetComponent<TweenScale> ().PlayForward ();
				resultText.GetComponent<UIInput> ().value = s;
			}
		}
		w.Dispose ();
	}	

	/// <summary>
	/// Gets the class string.
	/// </summary>
	/// <returns>The class string.</returns>
	/// <param name="go">Go.</param>
	private string GetClassString(GameObject go){
		Transform table = go.transform.Find ("Table");
		BangItem[] items = table.GetComponentsInChildren<BangItem> ();
		List<string> ls = new List<string> ();
		foreach (BangItem item in items) {
			ls.Add (item.label.text);
		}
		go.transform.DestroyChildren ();
		return string.Join (",", ls.ToArray ());
	}

	private IEnumerator UploadRecord(string content,string time,string place,string participant,string _event,string remarks){
		WWWForm form = new WWWForm ();
		form.AddField ("userid", PlayerPrefs.GetInt("userid"));
		form.AddField ("content", content);
		form.AddField ("time", time);
		form.AddField ("place", place);
		form.AddField ("participant", participant);
		form.AddField ("event", _event);
		form.AddField ("remarks", remarks);
		WWW w = new WWW (url+"addrecord", form);
		yield return w;
		if (w.error != null) {
			print (w.error);
		} else {
			print (w.text);
			if (w.text == "true") {
				StartCoroutine (LoadContent ());
				classDialog.GetComponent<TweenScale> ().PlayForward ();
				contentListDialog.GetComponent<TweenScale> ().PlayReverse ();
				finshButton.gameObject.SetActive (false);
				speakButton.gameObject.SetActive (true);
				testButton.gameObject.SetActive (true);
				Log ("记录成功");
			} else {
				Log ("记录失败,请重新尝试");
			}
		}
		w.Dispose ();
	}

	/// <summary>
	/// 上传语句分词voiceinfotoken
	/// </summary>
	/// <param name="s">S.</param>
	private IEnumerator Spilt(string s){
		WWWForm form = new WWWForm ();
		form.AddField ("voiceinfo", s);
		WWW w = new WWW (url + "voiceinfotoken", form);
		yield return w;
		if (w.error != null) {
			print (w.error);
		} else {
			print (w.text);
			string temp = w.text.Substring (4, w.text.Length - 4);
			print (temp);
			JsonData data = JsonMapper.ToObject (temp);
			ShowBangItem (data);
		}
		w.Dispose ();
	}

	/// <summary>
	/// 加载空间内容
	/// </summary>
	/// <returns>The content.</returns>
	private IEnumerator LoadContent(string keyword=null){
		WWWForm form = new WWWForm ();
		form.AddField ("userid", id);
		if (keyword != null) {
			form.AddField ("keyword", keyword);
		}
		WWW w = new WWW (url + "getrecord", form);
		yield return w;
		if (w.error != null) {
			print (w.error);
		} else {
			Debug.Log (w.text);
			contentGrid.transform.DestroyChildren ();
			JsonData data = JsonMapper.ToObject (w.text);
			for (int i = 0; i < data.Count; i++) {
				GameObject gridItem = NGUITools.AddChild (contentGrid, (GameObject)(Resources.Load ("ContentItem")));
				gridItem.GetComponent<ContentItem> ().Initialize ((string)data[i]["time"],(string)data[i]["place"],(string)data[i]["participant"],(string)data[i]["event"],(string)data[i]["remarks"],(string)data[i]["content"]);
				
			}
			UIGrid ngui_ui_grid = contentGrid.GetComponent<UIGrid> ();
			ngui_ui_grid.repositionNow = true;
		}
		w.Dispose ();
	}
}
