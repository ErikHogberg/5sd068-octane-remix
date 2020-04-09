using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GasNeedleUIScript : MonoBehaviour {
	public static NeedleMeterUIScript meter;
	public static TMP_Text kmph;

	void Awake() {
		meter = GetComponent<NeedleMeterUIScript>();
		kmph = transform.parent.transform.Find("TextKmph").GetComponent<TMP_Text>();
	}

	public static void Refresh() {
		if (meter == null)
			return;
		meter.SetBarPercentage();
		meter.ApplyColor();
	}

	public static void SetBarPercentage(float percentage, bool is_boosting) {
		if (meter == null)
			return;

		meter.SetTargetPercent(percentage);
		if (is_boosting)
			meter.SetTargetColor(NeedleMeterUIScript.ColorState.BOOST);
		else {
			if (percentage > 0.97f)
				meter.SetTargetColor(NeedleMeterUIScript.ColorState.MAX);
			else
				meter.SetTargetColor(NeedleMeterUIScript.ColorState.NORMAL);
		}
		Refresh();
	}

	public static void SetKMPH (float speed) {
		if (kmph == null)
			return;
		kmph.text = speed.ToString("F2") + " km/h";
    }
}
