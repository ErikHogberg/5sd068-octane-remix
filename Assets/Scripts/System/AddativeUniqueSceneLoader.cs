using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AddativeUniqueSceneLoader : MonoBehaviour {
	private static AddativeUniqueSceneLoader mainInstance;

	public string SceneToAdd;

	private void Awake() {
		if (mainInstance) {
			Destroy(gameObject);
			return;
		}

		SceneManager.LoadScene(SceneToAdd, LoadSceneMode.Additive);
		mainInstance = this;
	}

	private void OnDestroy() {
		if (mainInstance == this) {
			mainInstance = null;
		}
	}
}
