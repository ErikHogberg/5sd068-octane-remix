using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class CharacterSelectInputDelegateScript : MonoBehaviour {

	[Tooltip("Press right")]
	public UnityEvent NextCarEvents;
	[Tooltip("Press left")]
	public UnityEvent PreviousCarEvents;
	[Space]
	[Tooltip("Up/down event when entering confirm mode")]
	public UnityEvent EnterConfirmModeEvents;
	[Tooltip("Up/down event when leaving confirm mode")]
	public UnityEvent LeaveConfirmModeEvents;
	[Space]
	[Tooltip("Accept event when not in confirm mode")]
	public UnityEvent SelectCarEvents;
	[Tooltip("Accept event when in confirm mode")]
	public UnityEvent NextSceneEvents;
	[Tooltip("Cancel press events, is not affected by confirm mode or anything else")]
	public UnityEvent CancelEvents;

	[Space]
	public InputActionReference UpBinding;
	public InputActionReference DownBinding;
	public InputActionReference LeftBinding;
	public InputActionReference RightBinding;

	[Space]

	public InputActionReference AcceptBinding;
	public InputActionReference CancelBinding;

	[Space]
	public bool ConfirmMode = false;

	[Space]
	[Tooltip("If pressing left or right while in confirm mode will cause you to leave confirm mode")]
	public bool LeaveConfirmModeOnLeftRight = false;
	[Tooltip("If leaving confirm mode using left or right will also call the next or previous call events")]
	public bool CallLeftRightOnLeaveConfirm = false;


	bool CheckLeftRight() {
		if (ConfirmMode) {
			if (LeaveConfirmModeOnLeftRight) {
				ConfirmMode = false;
				LeaveConfirmModeEvents.Invoke();
				return CallLeftRightOnLeaveConfirm;
			} else {
				return false;
			}
		}
		return true;
	}

	void Next() {
		if (CheckLeftRight())
			PreviousCarEvents.Invoke();
	}

	void Previous() {
		if (CheckLeftRight())
			NextCarEvents.Invoke();
	}

	void ToggleConfirmMode() {
		ConfirmMode = !ConfirmMode;
		if (ConfirmMode) {
			EnterConfirmModeEvents.Invoke();
		} else {
			LeaveConfirmModeEvents.Invoke();
		}
	}

	void Accept() {
		if (ConfirmMode) {
			NextSceneEvents.Invoke();
		} else {
			SelectCarEvents.Invoke();
		}
	}


	private void Awake() {
		UpBinding.action.performed += _ => ToggleConfirmMode();
		DownBinding.action.performed += _ => ToggleConfirmMode();
		LeftBinding.action.performed += _ => Previous();
		RightBinding.action.performed += _ => Next();

		AcceptBinding.action.performed += _ => Accept();
		CancelBinding.action.performed += _ => CancelEvents.Invoke();


		UpBinding.action.Enable();
		DownBinding.action.Enable();
		LeftBinding.action.Enable();
		RightBinding.action.Enable();

		AcceptBinding.action.Enable();
		CancelBinding.action.Enable();
	}

	private void OnDestroy() {
		UpBinding.action.Disable();
		DownBinding.action.Disable();
		LeftBinding.action.Disable();
		RightBinding.action.Disable();

		AcceptBinding.action.Disable();
		CancelBinding.action.Disable();
	}

}
