using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(ObjectSelectorScript))]
public class CameraSelectorScript : MonoBehaviour {

	ObjectSelectorScript selector;

	private void Awake() {
		selector = GetComponent<ObjectSelectorScript>();
	}

	void Update() {

		if (Keyboard.current.digit1Key.wasPressedThisFrame)
			selector.UnhideObject(0);
		else if (Keyboard.current.digit2Key.wasPressedThisFrame)
			selector.UnhideObject(1);
		else if (Keyboard.current.digit3Key.wasPressedThisFrame)
			selector.UnhideObject(2);
		else if (Keyboard.current.digit4Key.wasPressedThisFrame)
			selector.UnhideObject(3);

	}
}
