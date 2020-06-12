using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FastModeToggleScript : MonoBehaviour {

	void Start() {
        GetComponent<Toggle>().isOn = SteeringScript.EnableProfileChange;
	}

	public void Toggle(bool value) {
		SteeringScript.EnableProfileChange = value;
	}

}
