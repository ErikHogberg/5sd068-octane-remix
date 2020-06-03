using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneUIScript : MonoBehaviour {

	public string CurrentScene;

	private void Awake() {
		if (!SceneManager.GetSceneByName(CurrentScene).IsValid()) {
			string oldCurrentScene = CurrentScene;
			CurrentScene = SceneManager.GetActiveScene().name;
			Debug.LogWarning("Current scene (" + oldCurrentScene + ") not found, using \"" + CurrentScene + "\" instead");
		}
		// if (DebugOutput) {
		// 	Debug.Log("Current scene: " + currentScene);
		// }
	}

	public void StartScene(string sceneName) {
		Time.timeScale = 1f;
		SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
	}

	// AsyncOperation loadSceneProgress = null;
	// AsyncOperation unloadSceneProgress = null;

	bool loading = false;

	public void SwapCurrentScene(string SceneToSwapTo) {
		if (loading)
			return;

		loading = true;

		if (!SceneManager.GetSceneByName(CurrentScene).IsValid()) {
			Debug.LogError("Chosen current scene does not exist: " + CurrentScene);
			Quit();
			return;
		}

		// var unloadProgress =
		// var unloadSceneProgress = 
		 SceneManager.UnloadSceneAsync(CurrentScene);
		// StartCoroutine(UnloadScene(CurrentScene));

		// unloadSceneProgress.allowSceneActivation = false;
		// var loadProgress = 
		// var loadSceneProgress = 
		SceneManager.LoadSceneAsync(SceneToSwapTo, LoadSceneMode.Additive);
		// loadSceneProgress.allowSceneActivation = false;
		// StartCoroutine(LoadScene(SceneToSwapTo));


	}

	// private void Update() {
	// 	if (loadSceneProgress.isDone) {
	// 		loadSceneProgress.allowSceneActivation = true;
	// 		return;
	// 	}
	// 	SceneLoadBarScript.SetProgress(loadSceneProgress.progress, unloadSceneProgress.progress);
	// }

	IEnumerator LoadScene(string sceneToLoad) {
		yield return null;

		//Begin to load the Scene you specify
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
		//Don't let the Scene activate until you allow it to
		asyncOperation.allowSceneActivation = false;
		// Debug.Log("Pro :" + asyncOperation.progress);
		// SceneLoadBarScript.SetProgress(asyncOperation.progress, 0.5f);

		//When the load is still in progress, output the Text and progress bar
		while (!asyncOperation.isDone) {
			//Output the current progress
			// m_Text.text = "Loading progress: " + (asyncOperation.progress * 100) + "%";
			SceneLoadBarScript.SetProgress(asyncOperation.progress, 0.5f);

			// Check if the load has finished
			if (asyncOperation.progress >= 0.9f) {
				//Change the Text to show the Scene is ready
				// m_Text.text = "Press the space bar to continue";
				//Wait to you press the space key to activate the Scene
				// if (Input.GetKeyDown(KeyCode.Space))
				//Activate the Scene
				asyncOperation.allowSceneActivation = true;
			}

			yield return null;
		}
	}

	IEnumerator UnloadScene(string sceneToUnload) {
		yield return null;

		AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(sceneToUnload);
		asyncOperation.allowSceneActivation = false;
		while (!asyncOperation.isDone) {
			SceneLoadBarScript.SetProgress(asyncOperation.progress, 0.5f);

			if (asyncOperation.progress >= 0.9f) {
				asyncOperation.allowSceneActivation = true;
			}

			yield return null;
		}
	}

	public void Quit() {
#if UNITY_STANDALONE
		Application.Quit();
#endif

#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#endif
	}

}
