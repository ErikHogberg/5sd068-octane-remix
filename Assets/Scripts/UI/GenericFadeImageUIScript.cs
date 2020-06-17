using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MaskableGraphic))]
public class GenericFadeImageUIScript : MonoBehaviour {

	public enum FadeMode {
		None,
		FadeIn,
		FadeOut,
	}

	public FadeMode StartMode = FadeMode.None;
	private FadeMode mode;

	[Header("Fade In")]

	[Min(0f)]
	public float FadeInTime;
	public Color FadeInFromColor;
	public AnimationCurve FadeInCurve;

	[Header("Fade Out")]

	[Min(0f)]
	public float FadeOutTime;
	public Color FadeOutToColor;
	public AnimationCurve FadeOutCurve;

	// [Space]
	// public GameObject OptionalParent;

	MaskableGraphic image;
	Color initColor;
	float timer = -1;
	
	public bool Running => timer >= 0;

	void Start() {
		image = GetComponent<MaskableGraphic>();
		initColor = image.color;

		mode = StartMode;

		switch (StartMode) {
			case FadeMode.FadeIn:
				FadeIn();
				break;
			case FadeMode.FadeOut:
				FadeOut();
				break;
		}

		Hide();
	}

	void Update() {

		if (timer < 0)
			return;

		timer -= Time.unscaledDeltaTime;

		switch (mode) {
			case FadeMode.None:
				timer = -1;
				return;
			case FadeMode.FadeIn: {
					float percentage = FadeInCurve.Evaluate(Mathf.Clamp01(timer / FadeInTime));
					image.color = Color.Lerp(initColor, FadeInFromColor, percentage);
					break;
				}
			case FadeMode.FadeOut: {
					float percentage = FadeOutCurve.Evaluate(Mathf.Clamp01(timer / FadeOutTime));
					image.color = Color.Lerp(FadeOutToColor, initColor, percentage);
					break;
				}
		}
	}

	public void FadeIn() {
		Show();
		timer = FadeInTime;
		mode = FadeMode.FadeIn;
	}

	public void FadeOut() {
		Show();
		timer = FadeOutTime;
		mode = FadeMode.FadeOut;
	}

	public void SetVisible(bool visible) {
		image.enabled = visible;
		// if (OptionalParent)
		// 	OptionalParent.SetActive(visible);
		// else
		// 	gameObject.SetActive(visible);
	}

	public void Show() {
		SetVisible(true);
	}

	public void Hide() {
		SetVisible(false);
	}

}
