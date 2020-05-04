using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public enum UIMode
{
	HOME = 0,
	REMIX,
	CHARSELECT
}

public class UINavListener
{
	protected UIMode mode;
	protected bool swipeReady = true;

	protected float lastSwipeValue = 0f;
	protected float lastConfirmValue = 0f;
	protected float lastCancelValue = 0f;

	public void SwipePing(float swipe) { lastSwipeValue = swipe; ModeSwipe(); }
	public void ConfirmPing(float confirm) { lastConfirmValue = confirm; ModeConfirm(); }
	public void CancelPing(float cancel) { lastCancelValue = cancel; ModeCancel(); }

    public UINavListener(UIMode p_mode) { mode = p_mode; }

	protected void ModeSwipe() {
		if (mode == UIMode.HOME) { }
		else if (mode == UIMode.REMIX) { }
		else if (mode == UIMode.CHARSELECT) 
		{
			Debug.Log("Swipe: " + lastSwipeValue);
			if (swipeReady) {
				if (lastSwipeValue <= -0.5f) {
					//CharacterSelection.i.SwapDisplayCar(true);
					swipeReady = false;
				} else if (lastSwipeValue >= 0.5f) {
					//CharacterSelection.i.SwapDisplayCar(false);
				}
			}
			else { if (lastSwipeValue == 0f) swipeReady = true; }
		}
    }
	protected void ModeConfirm() {
		if (mode == UIMode.HOME) { }
		else if (mode == UIMode.REMIX) { }
		else if (mode == UIMode.CHARSELECT) {
			Debug.Log("Confirm: " + lastConfirmValue);
			if (lastConfirmValue >= 0.5f) CharacterSelection.i.Test();
				//CharacterSelection.i.MakePick(0, CharacterSelection.i.CurrentIndex());
		}
	}
	protected void ModeCancel() {
		if (mode == UIMode.HOME) { }
		else if (mode == UIMode.REMIX) { }
		else if (mode == UIMode.CHARSELECT) {
			Debug.Log("Cancel: " + lastCancelValue);
			if (lastCancelValue >= 0.5f) CharacterSelection.i.ActivateCharSelect(false);
		}
	}

	public void ChangeUIMode(UIMode p_mode) { mode = p_mode; }
}

public class UINavInput : MonoBehaviour
{
	private static UINavInput _i;
	public static UINavInput i {
		get {
			if (_i == null) { _i = Instantiate(Resources.Load<UINavInput>("UINav")); }
			return _i;
		}
	}

	[Header("Key bindings")]
	public InputActionReference sideSwipeKeyBind;
	public InputActionReference confirmKeyBind;
	public InputActionReference cancelKeyBind;

	protected float swipeBuffer = 0f;
	protected float confirmBuffer = 0f;
	protected float cancelBuffer = 0f;

	private UIMode currentMode;
	private Dictionary<UIMode, UINavListener> listeners;

	private void SetSwipe(CallbackContext c) {
		float input = c.ReadValue<float>();
		swipeBuffer = input;
		foreach (KeyValuePair<UIMode, UINavListener> listener in listeners) {
			listener.Value.SwipePing(swipeBuffer);
        }
	}

	private void SetConfirm(CallbackContext c) {
		float input = c.ReadValue<float>();
		confirmBuffer = input;
		foreach (KeyValuePair<UIMode, UINavListener> listener in listeners) {
			listener.Value.ConfirmPing(confirmBuffer);
		}
	}

	private void SetCancel(CallbackContext c) {
		float input = c.ReadValue<float>();
		cancelBuffer = input;
		foreach (KeyValuePair<UIMode, UINavListener> listener in listeners) {
			listener.Value.CancelPing(cancelBuffer);
		}
	}

	void Awake() {
		sideSwipeKeyBind.action.performed += SetSwipe;
		confirmKeyBind.action.performed += SetConfirm;
		cancelKeyBind.action.performed += SetCancel;

		currentMode = UIMode.HOME;
		listeners = new Dictionary<UIMode, UINavListener>();
		Deactivate();
	}

	public void Activate() {
		sideSwipeKeyBind.action.Enable();
		confirmKeyBind.action.Enable();
		cancelKeyBind.action.Enable();
	}

	public void Deactivate() {
		sideSwipeKeyBind.action.Disable();
		confirmKeyBind.action.Disable();
		cancelKeyBind.action.Disable();
	}

	public void SetUINavMode(UIMode p_mode) { currentMode = p_mode; }
	public void AddUINavListener(UIMode p_mode) {
		if (listeners.ContainsKey(p_mode)) {
			Debug.Log("UINavInput: Listeners list already contains an entry for this UI mode");
		}
		else {
			listeners.Add(p_mode, new UINavListener(p_mode));
		}
	}
}
