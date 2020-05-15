using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ObjectSelectorScript))]
public class CameraSelectorScript : MonoBehaviour {

	ObjectSelectorScript selector;

	private void Awake() {
		selector = GetComponent<ObjectSelectorScript>();
	}

	void Update() {

		if (Input.GetKeyDown(KeyCode.Alpha1))
			selector.UnhideObject(0);
		else if (Input.GetKeyDown(KeyCode.Alpha2))
			selector.UnhideObject(1);
		else if (Input.GetKeyDown(KeyCode.Alpha3))
			selector.UnhideObject(2);
		else if (Input.GetKeyDown(KeyCode.Alpha4))
			selector.UnhideObject(3);

	}
}
