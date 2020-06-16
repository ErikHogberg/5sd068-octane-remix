using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class UINotificationSystem : MonoBehaviour {

	public static UINotificationSystem MainInstance = null;

	private enum FadeMode {
		FadeIn,
		FadeOut,
		Wait,
	}

	private FadeMode mode = FadeMode.FadeIn;

	public float FadeInTime;
	public AnimationCurve FadeInCurve;

	public float FadeOutTime;
	public AnimationCurve FadeOutCurve;

	private TMP_Text textComponent;

	private float timer = -1;
	private float waitTime = 1;
	private Color targetColor;

	private void Awake() {
		MainInstance = this;
		textComponent = GetComponent<TMP_Text>();
		textComponent.enabled = false;
	}

	private void OnDestroy() {
		MainInstance = null;
	}

	private void Update() {
		if (timer < 0)
			return;

		timer -= Time.deltaTime;

		switch (mode) {
			case FadeMode.FadeIn: {
					float percentage = FadeInCurve.Evaluate(Mathf.Clamp(timer / FadeInTime, 0, 1));
					Color color = Color.Lerp(targetColor, Color.clear, percentage);
					textComponent.color = color;
					if (timer < 0) {
						mode = FadeMode.Wait;
						timer = waitTime;
					}
					break;
				}
			case FadeMode.FadeOut: {
					float percentage = FadeOutCurve.Evaluate(Mathf.Clamp(timer / FadeOutTime, 0, 1));
					Color color = Color.Lerp(targetColor, Color.clear, 1 - percentage);
					textComponent.color = color;
					if (timer < 0)
						textComponent.enabled = false;
					break;
				}
			case FadeMode.Wait:
				if (timer < 0) {
					mode = FadeMode.FadeOut;
					timer = FadeOutTime;
				}
				break;
		}

	}

	private void Show(string text, Color color, float duration) {
		mode = FadeMode.FadeIn;
		timer = FadeInTime;
		waitTime = duration;
		targetColor = color;
		textComponent.text = text;
		textComponent.enabled = true;
	}

	public static void Notify(string text, Color color, float duration) {
		if (!MainInstance)
			return;

		MainInstance.Show(text, color, duration);
	}

}
