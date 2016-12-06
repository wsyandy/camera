using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class UIController : MonoBehaviour {

	public UISprite headSprite,kuang,destination,card,logoCard,dialog;
	public int gps=0,logo=1,good=2;
	public UILabel debugLabel,dialogLabel;

	private Dictionary<string,UISprite> spriteDict;
	private int userid;
	private string url;
	// Use this for initialization
	void Start () {
		spriteDict = new Dictionary<string, UISprite> ();
		spriteDict.Add ("head", headSprite);
		spriteDict.Add ("kuang", kuang);
		spriteDict.Add ("destination", destination);
		spriteDict.Add ("card", card);
		string spritName = PlayerPrefs.GetString ("image");
		headSprite.spriteName = spritName;
		url = PlayerPrefs.GetString ("url");
		userid = PlayerPrefs.GetInt ("userid");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
		
	public void SetSpriteStatus(string spriteName,bool isShow,Vector2 v2=default(Vector2),string labelText=""){
		UISprite us = spriteDict [spriteName];
		us.gameObject.SetActive (isShow);
		if (v2 != default(Vector2)) {
			us.transform.localPosition = v2;
		}
		if (labelText != "") {
			us.transform.Find ("CardLabel").gameObject.GetComponent<UILabel> ().text = labelText;
		}
	}

	public void LogoClick(int type,string name,string content=""){
		StartCoroutine (AddCard(type,name,content));
	}

	IEnumerator AddCard(int type,string name,string content){
		bool no_have = true;
		WWWForm form = new WWWForm ();
		form.AddField ("userid", userid);
		WWW w = new WWW (url+"getequipmentlist", form);
		yield return w;
		if (w.error != null) {
			print (w.error);
		} else {
			print (w.text);
			JsonData data = JsonMapper.ToObject (w.text);
			for (int i = 0; i < data.Count; i++) {
				if (name == (string)data [i]["cardtitle"]) {
					no_have = false;
				}
			}
		}
		w.Dispose ();
		if (no_have) {
			form = new WWWForm ();
			form.AddField ("userid", userid);
			form.AddField ("equipmenttype", type);
			form.AddField ("cardtitle", name);
			form.AddField ("cardcontent", content);
			w = new WWW (url + "addequipment", form);
			yield return w;
			if (w.error != null) {
				print (w.error);
			} else {
				print (w.text);
			}
			w.Dispose ();
			debugLabel.text = "卡牌添加成功";
		} else {
			debugLabel.text = "已有该类型卡牌";
		}
	}
}
