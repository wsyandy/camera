using UnityEngine;
using System.Collections;

public class MyDragAndDropItem :  UIDragDropItem {
	protected override void OnDragDropRelease(GameObject surface){
		base.OnDragDropRelease (surface);

		this.transform.parent = surface.transform.FindChild ("Table").transform;
		surface.GetComponentInChildren<UITable> ().Reposition ();
	}
}
