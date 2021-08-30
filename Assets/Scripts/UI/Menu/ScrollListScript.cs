using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(RectTransform))]
public class ScrollListScript : MonoBehaviour {

	private RectTransform rect;

	private float scroll = 0;
	private float maxScroll;

	public float ScrollSpeed = 1;

	public List<RectTransform> Items;

	void Start() {
		maxScroll = Items.Count * 10f;
		rect = GetComponent<RectTransform>();

		UpdateItemPos();

	}

	void Update() {
		float scrollDelta = Mouse.current.scroll.ReadValue().y;
		if (scrollDelta != 0) {
			scroll -= scrollDelta * ScrollSpeed;
			if (scroll < 0) scroll = 0;
			if (scroll > maxScroll) scroll = maxScroll;
			UpdateItemPos();
			Debug.Log("Scroll: " + scrollDelta);
		}
	}

	private void UpdateItemPos() {
		float scrollPercent = scroll / maxScroll;
		float height = rect.rect.height;

		for (int i = 0; i < Items.Count; i++) {
			float itemPercent = scrollPercent - ((float)i / Items.Count + 1);

			// TODO: set pos
			// TODO: set render depth

			Items[i].position = rect.position + new Vector3(0, rect.rect.yMax + height * itemPercent, 0);

		}
	}
}
