using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class ListItemScrollPing : MonoBehaviour {
	private ScrollToSelected scrollMaster = null;
	private RectTransform me = null;

	void Awake() {
		me = GetComponent<RectTransform>();
	}

	public void RegisterScrollMaster(ScrollToSelected scroll) {
		scrollMaster = scroll;
	}

	public void SendSelectedPing() {
		if (scrollMaster == null) {
			Debug.Log("ScrollPing: No ScrollToSelected script registered as master");
			return;
		} else {
			scrollMaster.SnapTo(me);
		}
	}
}
