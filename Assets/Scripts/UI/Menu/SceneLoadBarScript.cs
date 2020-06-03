using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BarUIScript))]
public class SceneLoadBarScript : MonoBehaviour {

	private static SceneLoadBarScript mainInstance;

	public GameObject ObjectToHide;

	// private BarUIScript bar;
	public BarUIScript loadBar;
	public BarUIScript unloadBar;

	private void Awake() {
		mainInstance = this;
		// bar = GetComponent<BarUIScript>();
		Hide();
	}

	private void OnDestroy() {
		if (mainInstance == this) {
			mainInstance = null;
		}
	}

	public static void SetVisible(bool visible) {
		if (!mainInstance)
			return;

		if (mainInstance.ObjectToHide) {
			mainInstance.ObjectToHide.SetActive(visible);
		} else {
			mainInstance.transform.parent.gameObject.SetActive(visible);
		}

	}

	public static void Show() {
		SetVisible(true);
	}

	public static void Hide() {
		SetVisible(false);
	}

	public static void SetProgress(float loadProgress, float unloadProgress) {
		if (!mainInstance)
			return;

		Show();
		mainInstance.loadBar.SetBarPercentage(loadProgress);
		mainInstance.unloadBar.SetBarPercentage(unloadProgress);
	}


}
