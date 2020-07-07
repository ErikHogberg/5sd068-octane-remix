using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class birthdayTriggerScript : MonoBehaviour {
	public GameObject[] ToDisable;
	public GameObject[] ToEnable;

	void Start() {

	}

	private void OnTriggerEnter(Collider other) {
		foreach (var item in ToDisable) {
			item.SetActive(false);
		}

		foreach (var item in ToEnable) {
			item.SetActive(true);
		}
	}

}
