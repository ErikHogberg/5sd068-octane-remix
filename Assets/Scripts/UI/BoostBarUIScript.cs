using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BarUIScript))]
public class BoostBarUIScript : MonoBehaviour {

	public static BarUIScript bar;

	void Awake() {
		bar = GetComponent<BarUIScript>();
	}

	public static void SetBarPercentage(float percentage, Color? color = null) {
		if (bar == null)
			return;

		bar.SetBarPercentage(percentage);

		if (color.HasValue) 
			bar.SetColor(color.Value);
		
	}

}
