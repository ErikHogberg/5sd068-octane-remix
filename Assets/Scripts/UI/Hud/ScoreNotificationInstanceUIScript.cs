using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreNotificationInstanceUIScript : MonoBehaviour {

	public TMP_Text Text;
	[Space]
	public Vector3 MoveSpeed = Vector3.up;
	public float Duration = 2f;
	public AnimationCurve FadeCurve = AnimationCurve.Linear(0, 0, 1, 1);

	float timer = -1;
	public bool running => timer > 0;

	Color initColor;
	Color targetColor = Color.clear;

	public void Show(Vector3 position, string message, Color color) {
		timer = Duration;
		Text.text = message;
		Text.color = color;
		initColor = color;
		gameObject.SetActive(true);
		transform.position = position;
	}

	void Update() {
		if (timer < 0) {
			gameObject.SetActive(false);
			return;
		}

		timer -= Time.unscaledDeltaTime;

		transform.Translate(MoveSpeed, Space.World);

		Text.color = Color.Lerp(initColor, targetColor, FadeCurve.Evaluate(1f - Mathf.Clamp01(timer / Duration)));

	}
}
