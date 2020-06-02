using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneUIScript : MonoBehaviour {

	public string CurrentScene;

	private void Awake() {
		if (!SceneManager.GetSceneByName(CurrentScene).IsValid()) {
			CurrentScene = SceneManager.GetActiveScene().name;
			Debug.LogWarning("Current scene not found, using \"" + CurrentScene + "\" instead");
		}
		// if (DebugOutput) {
		// 	Debug.Log("Current scene: " + currentScene);
		// }
	}

	public void StartScene(string sceneName) {
		Time.timeScale = 1f;
		SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
	}

	public void SwapCurrentScene(string SceneToSwapTo) {
		if (!SceneManager.GetSceneByName(CurrentScene).IsValid()) {
			Debug.LogError("Chosen current scene does not exist: " + CurrentScene);
			Quit();
			return;
		}

		// var unloadProgress =
		SceneManager.UnloadSceneAsync(CurrentScene);
		// var loadProgress = 
		SceneManager.LoadSceneAsync(SceneToSwapTo, LoadSceneMode.Additive);



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
