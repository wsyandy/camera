using UnityEngine;
using System.Collections;

public class ContentItem : MonoBehaviour {

	public UILabel time, place, who, events, remark, content;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	/// <summary>
	/// 初始化记录模块
	/// </summary>
	/// <param name="t">时间</param>
	/// <param name="p">地点</param>
	/// <param name="w">人物</param>
	/// <param name="e">事件</param>
	/// <param name="r">备注</param>
	/// <param name="c">内容</param>
	public void Initialize(string t,string p,string w,string e,string r,string c){
		time.text = t;
		place.text = p;
		who.text = w;
		events.text = e;
		remark.text = r;
		content.text = c;
	}
}
