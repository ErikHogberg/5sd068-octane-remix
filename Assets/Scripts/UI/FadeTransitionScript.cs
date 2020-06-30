using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class FadeTransitionScript : MonoBehaviour {

	public static FadeTransitionScript MainInstance = null;

	Image fadePanel;

	public AnimationCurve FadeCurve = AnimationCurve.Linear(0, 0, 1, 1);
	public float FadeTime = 1f;

	public bool StartOnSpawn = false;

	bool fadeIn = false;

	float timer = -1;

	ChangeSceneUIScript sceneChanger = null;

	void Start() {
		MainInstance = this;

		fadePanel = GetComponent<Image>();

		if (StartOnSpawn) {
			timer = FadeTime;
			fadeIn = false;
		}
	}

	private void OnDestroy() {
		MainInstance = null;
	}

	void Update() {

		if (timer < 0) {
			gameObject.SetActive(false);
			sceneChanger?.ApplySwapCurrentScene();
			return;
		}

		float percentage = FadeCurve.Evaluate(Mathf.Clamp01(timer / FadeTime));
		if (fadeIn) {
			percentage = 1f - percentage;
		}

		fadePanel.color = Color.Lerp(Color.white, Color.clear, percentage);

	}

	public void Show(ChangeSceneUIScript sceneChanger) {
		this.sceneChanger = sceneChanger;
		fadeIn = true;
		timer = FadeTime;
		gameObject.SetActive(true);
	}
}
