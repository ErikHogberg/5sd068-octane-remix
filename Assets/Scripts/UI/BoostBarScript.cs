using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BarScript))]
public class BoostBarScript : MonoBehaviour {

	public static BarScript bar;

	void Awake() {
		bar = GetComponent<BarScript>();
	}

	public static void SetBarPercentage(float percentage, Color? color = null) {
		if (bar == null)
			return;

		bar.SetBarPercentage(percentage);

		if (color.HasValue) 
			bar.SetColor(color.Value);
		
	}

}
