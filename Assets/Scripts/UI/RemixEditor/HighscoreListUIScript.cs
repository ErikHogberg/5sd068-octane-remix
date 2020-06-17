using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighscoreListUIScript : MonoBehaviour {

	

	void Start() {

	}

	public void Toggle() {
		gameObject.SetActive(!gameObject.activeSelf);
	}

}
