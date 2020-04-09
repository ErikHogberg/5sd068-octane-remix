using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GasNeedleUIScript : MonoBehaviour {
	public static NeedleMeterUIScript meter;

	void Awake() {
		meter = GetComponent<NeedleMeterUIScript>();
	}

	public static void SetBarPercentage(float percentage, Color? color = null) {
		if (meter == null)
			return;

		meter.SetBarPercentage(percentage);

		if (color.HasValue)
			meter.SetColor(color.Value);

	}
}
