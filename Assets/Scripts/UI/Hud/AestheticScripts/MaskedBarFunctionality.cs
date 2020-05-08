using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class MaskedBarFunctionality : MonoBehaviour
{
	private float maxPos;
	private float minPos;
	private float diffPos;

	private float currPercent = 1.0f;
	private RectTransform thisTransform;

	void Awake() {
		thisTransform = GetComponent<RectTransform>();
		maxPos = thisTransform.anchoredPosition.x;
		minPos = maxPos - thisTransform.sizeDelta.x;
		diffPos = maxPos - minPos;
	}

	public void SetPos(float percent)
    {
		float newPos = minPos + (diffPos * percent);
		thisTransform.anchoredPosition = new Vector2(newPos, thisTransform.anchoredPosition.y);
		currPercent = percent;
	}

	public float GetPercent() { return currPercent; }
}
