using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UniquePrefabLoader : MonoBehaviour {
	private static UniquePrefabLoader mainInstance;

	public GameObject PrefabToLoad;

	private void Awake() {
		if (mainInstance) {
			Destroy(gameObject);
			return;
		}

		// SceneManager.LoadScene(SceneToAdd, LoadSceneMode.Additive);
		var instance = Instantiate(PrefabToLoad);
		mainInstance = this;
		DontDestroyOnLoad(gameObject);
		DontDestroyOnLoad(instance);
	}

	private void OnDestroy() {
		if (mainInstance == this) {
			mainInstance = null;
		}
	}
}
