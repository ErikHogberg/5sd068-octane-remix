using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
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

	[Header("Fade In")]

	[Min(0f)]
	public float FadeOutTime;
	public Color FadeOutToColor;
	public AnimationCurve FadeOutCurve;

	[Space]
	public GameObject OptionalParent;

	Image image;
	Color initColor;
	float timer = -1;

	void Start() {
		image = GetComponent<Image>();

		mode = StartMode;

		switch (StartMode) {
			case FadeMode.FadeIn:
				FadeIn();
				break;
			case FadeMode.FadeOut:
				FadeOut();
				break;
		}

	}

	void Update() {

		if (timer < 0)
			return;

		timer -= Time.deltaTime;

		switch (mode) {
			case FadeMode.None:
				timer = -1;
				return;
			case FadeMode.FadeIn: {
					float percentage = timer / FadeInTime;
					image.color = Color.Lerp(initColor, FadeInFromColor, 1f - percentage);
					break;
				}
			case FadeMode.FadeOut: {
					float percentage = timer / FadeOutTime;
					image.color = Color.Lerp(FadeOutToColor, initColor, 1f - percentage);
					break;
				}
		}




	}

	public void FadeIn() {
		timer = FadeInTime;
		mode = FadeMode.FadeIn;
	}

	public void FadeOut() {
		timer = FadeOutTime;
		mode = FadeMode.FadeOut;
	}

	public void SetVisible(bool visible) {
		if (OptionalParent)
			OptionalParent.SetActive(visible);
		else
			gameObject.SetActive(visible);
	}

	public void Show() {
		SetVisible(true);
	}

	public void Hide() {
		SetVisible(false);
	}

}
