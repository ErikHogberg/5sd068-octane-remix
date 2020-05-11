using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BarUIScript))]
public class BoostInvulnBarUIScript : MonoBehaviour {

	public static BarUIScript bar;

	void Awake() {
		bar = GetComponent<BarUIScript>();
	}

	private void Start() {
		SetBarPercentage(0);
	}

	public static void SetBarPercentage(float percentage) {
		if (bar == null)
			return;

		bar.SetBarPercentage(percentage);
	}

}
