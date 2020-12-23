using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostCarScript : MonoBehaviour {

	public static GhostCarScript MainInstance;

	public GameObject[] Cars;

	void Awake() {
		MainInstance = this;
	}

	void Start() {
		foreach (var item in Cars) {
			item.SetActive(false);
		}
	}

	void Update() {

	}

}
