using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using LitJson;

public class FriendController : MonoBehaviour {
	public UISprite[] friends,cards;
	public UILabel debugLabel,title,content;
	public UISprite messagBox;

	private int userid,friendid,cardid;
	private string url;


	void Awake(){
		userid = PlayerPrefs.GetInt ("userid");
		url = PlayerPrefs.GetString ("url");
		StartCoroutine (LoadFriend ());
		StartCoroutine (LoadCard ());
	}
	// Use this for initialization
	void Start () {
		friendid = 0;
		cardid = 0;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Back(){
		SceneManager.LoadScene ("camera", LoadSceneMode.Single);
	}

	public void FriendClick(Friend f){
		if (f.selectLogo.gameObject.activeSelf) {
			f.selectLogo.gameObject.SetActive (false);
			friendid = 0;
		} else {
			foreach (UISprite s in friends) {
				s.GetComponent<Friend> ().selectLogo.gameObject.SetActive (false);
			}
			f.selectLogo.gameObject.SetActive (true);
			friendid = int.Parse (f.id.text);
		}
	}

	public void CardClick(Card c){
		if (c.selectLogo.gameObject.activeSelf) {
			c.selectLogo.gameObject.SetActive (false);
			cardid = 0;
		} else {
			foreach (UISprite s in cards) {
				s.GetComponent<Card> ().selectLogo.gameObject.SetActive (false);
			}
			c.selectLogo.gameObject.SetActive (true);
			cardid = int.Parse (c.id.text);
		}
	}

	public void SendCardToFriend(){
		StartCoroutine (SendCardToFriendSync ());
	}

	public void DeleteCard(){
		StartCoroutine (DeleteCardSync ());
	}

	public void Logout(){
		PlayerPrefs.DeleteAll ();
		SceneManager.LoadScene ("start", LoadSceneMode.Single);
	}

	public void MessageClick(){
		if (messagBox.gameObject.activeSelf) {
			messagBox.gameObject.SetActive (false);
		} else {
			messagBox.gameObject.SetActive (true);
			StartCoroutine (LoadMessageSync());
		}
	}

	IEnumerator DeleteCardSync(){
		if (cardid != 0) {
			WWWForm form = new WWWForm ();
			form.AddField ("userid", userid);
			form.AddField ("equipmentid", cardid);
			WWW w = new WWW (url + "delequipment", form);
			yield return w;
			if (w.error != null) {
				print (w.error);
			} else {
				print (w.text);
			}
			w.Dispose ();
			StartCoroutine (LoadCard ());
		} else {
			debugLabel.text = "请选择卡牌";
		}
	}

	IEnumerator SendCardToFriendSync(){
		if (friendid != 0 && cardid != 0) {
			WWWForm form = new WWWForm ();
			form.AddField ("userid", userid);
			form.AddField ("friendid", friendid);
			form.AddField ("giftid", cardid);
			WWW w = new WWW (url + "sendgift", form);
			yield return w;
			if (w.error != null) {
				print (w.error);
			} else {
				print (w.text);
			}
			w.Dispose ();
			PlayerPrefs.SetInt ("send", 1);
			SceneManager.LoadScene ("camera", LoadSceneMode.Single);
//			StartCoroutine (LoadCard ());
		} else {
			debugLabel.text = "请选择朋友和卡牌";
		}
	}

	IEnumerator LoadFriend()
	{
		foreach (UISprite s in friends) {
			s.gameObject.SetActive (false);
		}
		WWWForm form = new WWWForm ();
		form.AddField ("userid", userid);
		WWW w = new WWW (url + "getfriendslist", form);
		yield return w;
		if (w.error != null) {
			print (w.error);
		} else {
			print (w.text);
			if (w.text != "None") {
				JsonData data = JsonMapper.ToObject (w.text);
				for (int i = 0; i < data.Count; i++) {
					friends [i].spriteName = (string)data [i] ["image"];
					Friend f = friends [i].GetComponent<Friend> ();
					int id = (int)data [i] ["userid"];
					f.id.text = id.ToString ();
					f.name.text = (string)data [i] ["name"];
					UIButton button = friends [i].GetComponent<UIButton> ();
					button.normalSprite = (string)data [i] ["image"];
					friends [i].gameObject.SetActive (true);
				}
			}
		}
		w.Dispose ();
	}

	IEnumerator LoadCard(){
		foreach (UISprite s in cards) {
			s.gameObject.SetActive (false);
		}
		WWWForm form = new WWWForm ();
		form.AddField ("userid", userid);
		WWW w = new WWW (url + "getequipmentlist", form);
		yield return w;
		if (w.error != null) {
			print (w.error);
		} else {
			print (w.text);
			if (w.text != "None") {
				JsonData data = JsonMapper.ToObject (w.text);
				for (int i = 0; i < data.Count; i++) {
					cards [i].spriteName = (string)data [i] ["cardtitle"];
					int id = (int)data [i] ["equipmentid"];
					cards [i].GetComponent<Card> ().id.text = id.ToString ();
					UIButton button = cards [i].GetComponent<UIButton> ();
					button.normalSprite = (string)data [i] ["cardtitle"];
					cards [i].gameObject.SetActive (true);
				}
			}
		}
		w.Dispose ();
	}

	IEnumerator LoadMessageSync(){
		WWW getdata = new WWW (url+"getcommoninterests");
		yield return getdata;
		if (getdata.error != null) {
			Debug.Log (getdata.error);
		} else {
			print (getdata.text);
			JsonData data=JsonMapper.ToObject(getdata.text);
			title.text=WWW.UnEscapeURL((string)data["title"]);
			content.text=WWW.UnEscapeURL((string)data["abstract"]);
		}
		getdata.Dispose ();
	}
}
