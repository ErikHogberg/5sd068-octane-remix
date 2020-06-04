using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PauseScript : MonoBehaviour {
	private static PauseScript mainInstance;

	public GameObject HideOnPause;
	public GameObject ShowOnPause;

	[Space]
	public InputActionReference PauseKeyBinding;


	private bool paused = false;

	private void Awake() {
		mainInstance = this;
		if (PauseKeyBinding != null) {
			PauseKeyBinding.action.performed += _ => Toggle();
			PauseKeyBinding.action.Enable();
		}
		Resume();
	}

	private void OnDestroy() {
		mainInstance = null;
		PauseKeyBinding?.action.Disable();
	}

	public void Pause() {
		paused = true;
		HideOnPause?.SetActive(false);
		ShowOnPause?.SetActive(true);
		SteeringScript.FreezeCurrentCar();
		EventSystem.current.SetSelectedGameObject(ShowOnPause.transform.GetChild(0).gameObject);
	}

	public void Resume() {
		paused = false;
		HideOnPause?.SetActive(true);
		ShowOnPause?.SetActive(false);
		if (!StartCountdownScript.IsShown)
			SteeringScript.UnfreezeCurrentCar();
	}

	public void Toggle() {
		if (paused) {
			Resume();
		} else {
			Pause();
		}
	}

	public static void PauseStatic() {
		mainInstance?.Pause();
	}

	public static void ResumeStatic() {
		mainInstance?.Resume();
	}

	public static void ToggleStatic() {
		mainInstance?.Toggle();
	}

}
