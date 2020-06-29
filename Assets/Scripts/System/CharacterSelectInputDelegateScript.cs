using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class CharacterSelectInputDelegateScript : MonoBehaviour {

	public TMP_InputField NameInput;
	[Space]

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

	bool CheckIfEditingNameInput() {
		if (!NameInput) {
			return false;
		} else {
			return NameInput.isFocused;
		}
	}

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
		if (CheckIfEditingNameInput())
			return;

		if (CheckLeftRight())
			PreviousCarEvents.Invoke();
	}

	void Previous() {
		if (CheckIfEditingNameInput())
			return;

		if (CheckLeftRight())
			NextCarEvents.Invoke();
	}

	void ToggleConfirmMode() {
		if (CheckIfEditingNameInput())
			return;

		ConfirmMode = !ConfirmMode;
		if (ConfirmMode) {
			EnterConfirmModeEvents.Invoke();
		} else {
			LeaveConfirmModeEvents.Invoke();
		}
	}

	void Accept() {
		if (CheckIfEditingNameInput())
			return;

		if (ConfirmMode) {
			NextSceneEvents.Invoke();
		} else {
			SelectCarEvents.Invoke();
		}
	}

	void Cancel() {
		if (CheckIfEditingNameInput())
			return;

		CancelEvents.Invoke();
	}


	private void Awake() {
		UpBinding.action.started += _ => ToggleConfirmMode();
		DownBinding.action.started += _ => ToggleConfirmMode();
		LeftBinding.action.started += _ => Previous();
		RightBinding.action.started += _ => Next();

		AcceptBinding.action.started += _ => Accept();
		CancelBinding.action.started += _ => Cancel();


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
