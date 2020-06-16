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
		SetVisible(false);
		if (mainInstance) {
			// Destroy(gameObject);
			return;
		}

		mainInstance = this;
		// bar = GetComponent<BarUIScript>();
		// Hide();
		// DontDestroyOnLoad(gameObject);
	}

	private void OnDestroy() {
		if (mainInstance == this) {
			mainInstance = null;
		}
	}

	public void SetVisible(bool visible) {
		if (ObjectToHide) {
			ObjectToHide.SetActive(visible);
		} else {
			transform.parent.gameObject.SetActive(visible);
		}
	}

	public static void SetVisibleStatic(bool visible) {
		if (!mainInstance)
			return;

		mainInstance.SetVisible(visible);
	}

	public static void Show() {
		SetVisibleStatic(true);
	}

	public static void Hide() {
		SetVisibleStatic(false);
	}

	public static void SetProgress(float loadProgress, float unloadProgress) {
		if (!mainInstance)
			return;

		Show();
		// Debug.Log("setting bar to " + loadProgress + ", " + unloadProgress);
		mainInstance.loadBar.SetBarPercentage(loadProgress);
		mainInstance.unloadBar.SetBarPercentage(unloadProgress);
	}


}
