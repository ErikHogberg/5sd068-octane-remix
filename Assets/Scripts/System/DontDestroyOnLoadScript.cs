using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyOnLoadScript : MonoBehaviour {

	private static DontDestroyOnLoadScript mainInstance;
	private bool clearOnDestroy = true;

	// IDEA: allow using same script, using a list with IDs, register object using ID set in editor
	// NOTE: might not be needed, as you can just have the object with the script as the parent of all objects that are kept between scenes

	private void Awake() {
		if (mainInstance != null) {
			clearOnDestroy = false;
			Destroy(gameObject);
			return;
		}

		mainInstance = this;
		DontDestroyOnLoad(gameObject);
	}

	private void OnDestroy() {
		if (clearOnDestroy)
			mainInstance = null;
	}

}
