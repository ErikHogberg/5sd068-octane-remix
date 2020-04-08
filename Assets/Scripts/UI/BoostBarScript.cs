using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BarScript))]
public class BoostBarScript : MonoBehaviour {

	public static BarScript bar;

	void Start() {
		bar = GetComponent<BarScript>();
	}

	public static void SetBarPercentage(float percentage) {
		if (bar != null)
			bar.SetBarPercentage(percentage);
	}

}
