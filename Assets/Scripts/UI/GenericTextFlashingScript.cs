using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class GenericTextFlashingScript : MonoBehaviour {

	public Color FlashColor;
	private Color initColor;

	private TMP_Text text;

	public AnimationCurve LerpCurve;

	[Range(0.001f, 300f)]
	public float FlashPerSecond = 1;
	private float oldFlashPerSecond;
	private float timer;
	private float maxTime;
	private bool forward = false;

	private void Start() {

		text = GetComponent<TMP_Text>();
		initColor = text.color;

		oldFlashPerSecond = FlashPerSecond;
		SetFlashRate(FlashPerSecond);
		
	}

	void Update() {

#if UNITY_EDITOR
		if (FlashPerSecond != oldFlashPerSecond)
			SetFlashRate(FlashPerSecond);
#endif

		if (forward) {
			timer += Time.deltaTime;
		} else {
			timer -= Time.deltaTime;
		}

		if (timer < 0) {
			timer = 0;
			forward = true;
		} else if (timer > maxTime) {
			timer = maxTime;
			forward = false;
		}

		float progress = LerpCurve.Evaluate(timer / maxTime);
		text.color = Color.Lerp(initColor, FlashColor, progress);
	}

	public void SetFlashRate(float newFlashPerSecond) {
		FlashPerSecond = newFlashPerSecond;
		timer = 1.0f / FlashPerSecond;
		maxTime = timer;
		forward = false;
	}

}
