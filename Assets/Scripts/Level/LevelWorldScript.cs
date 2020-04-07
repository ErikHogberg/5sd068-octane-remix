using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelWorldScript : MonoBehaviour {

	// IDEA: make private, only access through static methods with null checks
	public static LevelWorldScript CurrentLevel = null;

	public GameObject TestRespawnSpot;

	private void Awake() {
		CurrentLevel = this;
	}



}
