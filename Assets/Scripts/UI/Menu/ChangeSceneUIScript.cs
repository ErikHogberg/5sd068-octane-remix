using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneUIScript : MonoBehaviour {

	public static ChangeSceneUIScript MainInstance = null;

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

		if (MainInstance == null) {
			MainInstance = this;
		}
	}

	private void OnDestroy() {
		if (MainInstance == this) {
			MainInstance = null;
		}
	}

	public void StartScene(string sceneName) {
		Time.timeScale = 1f;
		SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
		// StartCoroutine(LoadScene(sceneName, false, LoadSceneMode.Single));
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

		// StartCoroutine(LoadScene(SceneToSwapTo, true, LoadSceneMode.Additive));


	}

	// private void Update() {
	// 	if (loadSceneProgress.isDone) {
	// 		loadSceneProgress.allowSceneActivation = true;
	// 		return;
	// 	}
	// 	SceneLoadBarScript.SetProgress(loadSceneProgress.progress, unloadSceneProgress.progress);
	// }

	IEnumerator LoadScene(string sceneToLoad, bool unloadCurrent, LoadSceneMode loadMode) {
		yield return null;

		SceneTransitionScript.MainInstance?.StartTransition();

		AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneToLoad, loadMode);
		AsyncOperation unloadOperation = null;

		loadOperation.allowSceneActivation = false;

		float unloadProgress = 1.0f;
		if (unloadCurrent) {
			unloadOperation = SceneManager.UnloadSceneAsync(CurrentScene);
			unloadOperation.allowSceneActivation = false;
			unloadProgress = unloadOperation.progress;
		}


		SceneLoadBarScript.SetProgress(loadOperation.progress, unloadProgress);


		while (!loadOperation.isDone || (!unloadOperation?.isDone ?? false)) {
			if (unloadCurrent) {
				unloadProgress = unloadOperation.progress;
			}
			SceneLoadBarScript.SetProgress(loadOperation.progress, unloadProgress);

			if (SceneTransitionScript.MainInstance?.Running ?? true) {
				if (loadOperation.progress >= 0.9f && !loadOperation.allowSceneActivation) {
					loadOperation.allowSceneActivation = true;
				}
				if (unloadCurrent && unloadOperation.progress >= 0.9f && !unloadOperation.allowSceneActivation) {
					unloadOperation.allowSceneActivation = true;
				}
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
