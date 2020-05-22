using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SegmentListItemScrollPing : MonoBehaviour
{
	private ScrollToSelected scrollMaster = null;
	private RectTransform me = null;

	void Awake() { me = GetComponent<RectTransform>(); }

	public void RegisterScrollMaster(ScrollToSelected p_scroll) {
		scrollMaster = p_scroll;
    }

	public void SendSelectedPing() {
		if (scrollMaster == null) {
			UnityEngine.Debug.Log("ScrollPing: No ScrollToSelected script registered as master");
			return;
		} else {
			scrollMaster.SnapTo(me);
		}
    }
}
