using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AddativeUniqueSceneLoader : MonoBehaviour {
	private static AddativeUniqueSceneLoader mainInstance;

	public string SceneToAdd;
	public bool IsSingletonInstance = true;

	private void Awake() {
		if (!IsSingletonInstance) {
			SceneManager.LoadScene(SceneToAdd, LoadSceneMode.Additive);
			return;
		}

		if (mainInstance) {
			Destroy(gameObject);
			return;
		}

		SceneManager.LoadScene(SceneToAdd, LoadSceneMode.Additive);
		mainInstance = this;
		DontDestroyOnLoad(gameObject);
	}

	private void OnDestroy() {
		if (IsSingletonInstance && mainInstance == this) {
			mainInstance = null;
		}
	}
}
