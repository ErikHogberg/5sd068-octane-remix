using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIObjectUtilitiesScript : MonoBehaviour {

	public void Show(GameObject target) {
		target.SetActive(true);
	}

	public void Hide(GameObject target) {
		target.SetActive(false);
	}

	public void ToggleActive(GameObject target) {
		target.SetActive(!target.activeSelf);
	}

}
