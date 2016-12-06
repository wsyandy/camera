using UnityEngine;
using System.Collections;
using LitJson;

public class ZoneGameController : MonoBehaviour {

	public UILabel debugLabel,buttonLabel,bangLabel;
	public UISprite ban,bigContainer;
	public UIButton bigBang;

	private JsonData data;
	private AudioSource myAudio;
	private GameObject speakButton, testButton, finshButton;

	void Start()
	{
		string[] ms = Microphone.devices;
		int deviceCount = ms.Length;
		if (deviceCount == 0)
		{
			Log("no microphone found");
		}
		myAudio = GetComponent<AudioSource> ();
		speakButton = GameObject.Find ("Speak");
		testButton = GameObject.Find ("Test");
		finshButton = GameObject.Find ("Finsh");
		finshButton.gameObject.SetActive (false);
	}
		
	public void SpeakClick(){
		if (buttonLabel.text == "开始录音") {
			buttonLabel.text = "停止录音";
			StartRecord ();
		} else {
			buttonLabel.text = "开始录音";
			StopRecord ();
			StartCoroutine (UploadVoice ());
		}
	}

	public void BigBang(){
		ban.gameObject.SetActive (true);
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
		myAudio.clip = Microphone.Start(null, true, 10, 10000);    
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
		string url = PlayerPrefs.GetString ("url");
		WWWForm form = new WWWForm ();
		form.AddBinaryData ("voice", GetClipData ());
		WWW w = new WWW (url + "voiceinfo", form);
		yield return w;
		if (w.error != null) {
			print (w.error);
		} else {
			print (w.text);
			if (w.text != "None") {
				string temp = w.text;
				temp = temp.Substring (4, temp.Length - 8);
				data = JsonMapper.ToObject (temp);
				string s = "";
				for (int i = 0; i < data .Count; i++) {
					s += data [i] ["cont"];
				}
				bigBang.gameObject.SetActive (true);
				bangLabel.text = s;
			}
		}
		w.Dispose ();
	}	

	public void ShowBangItem(){
		ban.gameObject.SetActive (false);
		bigContainer.gameObject.SetActive (true);
		GameObject itemGrid = GameObject.Find ("ItemGrid");
		for (int i = 0; i < data.Count; i++) {
			if ((string)data [i] ["pos"] != "wp") {
				GameObject gridItem = NGUITools.AddChild (itemGrid, (GameObject)(Resources.Load ("BangItemSprite")));
				gridItem.GetComponent<BangItem> ().label.text = (string)data [i] ["cont"];
			}
		}
		UIGrid ngui_ui_grid = itemGrid.GetComponent<UIGrid> ();
		ngui_ui_grid.repositionNow = true;
	}
}
