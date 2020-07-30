using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AddativeSceneLoader : MonoBehaviour {
	private static AddativeSceneLoader mainInstance;

	public enum LoaderMode {
		Unique,
		Placeholder,
		GenericLoader,
	}

	[Tooltip("[Unique]: If this loader takes up the unique singleton slot that will prevent other unique loaders. [Placeholder]: If this object takes up the singleton slot (if available) without loading a scene, only preventing further unique instances. [Generic]: If this loader always loads a level without checking or registering itself in the unique singleton slot")]
	public LoaderMode Mode = LoaderMode.Unique;

	public string SceneToAdd;
	// public bool IsSingletonInstance = true;
	// public bool IsPlaceholder = false;

	public bool UseTrackSelect = false;

	private void Awake() {

		if (UseTrackSelect) {
			switch (TrackSelectUIScript.SelectedTrack) {
				case "Long":
					SceneToAdd = "TrackScene";
					break;
				case "Short":
				default:
					SceneToAdd = "SecondTrackScene";
					break;
			}
		}

		switch (Mode) {
			case LoaderMode.Unique:
				if (mainInstance) {
					Destroy(gameObject);
					return;
				}
				SceneManager.LoadScene(SceneToAdd, LoadSceneMode.Additive);
				mainInstance = this;
				DontDestroyOnLoad(gameObject);
				break;
			case LoaderMode.Placeholder:
				if (mainInstance) {
					Destroy(gameObject);
					return;
				}
				mainInstance = this;
				DontDestroyOnLoad(gameObject);
				break;
			case LoaderMode.GenericLoader:
				SceneManager.LoadScene(SceneToAdd, LoadSceneMode.Additive);
				Destroy(gameObject);
				break;
		}

	}

	private void OnDestroy() {
		switch (Mode) {
			case LoaderMode.Unique:
				if (mainInstance == this) {
					mainInstance = null;
				}
				break;
			case LoaderMode.Placeholder:
				break;
			case LoaderMode.GenericLoader:
				break;
		}
	}
}
