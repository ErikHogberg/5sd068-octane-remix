using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonAnimations : MonoBehaviour
{
	public bool active = true;
	public float dimensionLerpSpeed = 10f;
	private float colorLerpSpeed = 9f;

	private Button thisButton;
	private Image thisBtnImage;
	private RectTransform thisTransform;

	private Vector2 originalDimensions;
	private float originalAlphaColor;

	private bool widthLerp = false;
	Coroutine widthLerpInstance;

	private bool alphaLerp = false;
	Coroutine alphaLerpInstance;


	void Awake() { 
		thisButton = GetComponent<Button>();
		thisBtnImage = GetComponent<Image>();
		thisTransform = GetComponent<RectTransform>();
		originalDimensions = thisTransform.sizeDelta;
	}

	public void WidthLerp(float p_goalPercent) {
		if (active) {
			if (widthLerp)
				StopCoroutine(widthLerpInstance);
			widthLerpInstance = StartCoroutine(Width(p_goalPercent));
		}
    }

	public void AlphaLerp(float p_goalPercent) {
		if (active) {
			if (alphaLerp)
				StopCoroutine(alphaLerpInstance);
			alphaLerpInstance = StartCoroutine(Alpha(p_goalPercent));
		}
    }


	IEnumerator Width(float goalPercent)
	{
		widthLerp = true;
		while (thisTransform.sizeDelta.x < (originalDimensions.x * goalPercent) - 1f || thisTransform.sizeDelta.x > (originalDimensions.x * goalPercent) + 1f)
		{
			float newX = Mathf.Lerp(thisTransform.sizeDelta.x, originalDimensions.x * goalPercent, dimensionLerpSpeed * Time.deltaTime);
			thisTransform.sizeDelta = new Vector2(newX, originalDimensions.y);
			yield return null;
		}
		thisTransform.sizeDelta = new Vector2(originalDimensions.x * goalPercent, originalDimensions.y);
		widthLerp = false;
		yield break;
	}

	IEnumerator Alpha(float goalPercent)
    {
		alphaLerp = true;
		while (thisBtnImage.color.a < (goalPercent - 0.005f) || thisBtnImage.color.a < (goalPercent + 0.005f))
		{
			float newA = Mathf.Lerp(thisBtnImage.color.a, goalPercent, colorLerpSpeed * Time.deltaTime);
			thisBtnImage.color = new Color(thisBtnImage.color.r, thisBtnImage.color.g, thisBtnImage.color.b, newA);
			yield return null;
		}
		thisBtnImage.color = new Color(thisBtnImage.color.r, thisBtnImage.color.g, thisBtnImage.color.b, goalPercent);
		alphaLerp = false;
		yield break;
	}
}
