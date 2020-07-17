using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class RumbleToggleUIScript : MonoBehaviour {

	Toggle toggle;

	void Start() {
		toggle = GetComponent<Toggle>();
		toggle.isOn = SteeringScript.EnableRumble;
	}

	public void Toggle(bool value) {
		SteeringScript.EnableRumble = value;
	}

}
