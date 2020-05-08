﻿using System.Collections;
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

	public static void SetBarPercentage(float percentage, bool isBoosting) {
		if (meter == null)
			return;

		meter.SetTargetPercent(percentage);
		if (isBoosting)
			meter.SetTargetColor(NeedleMeterUIScript.ColorState.BOOST);
		else {
			if (percentage > 0.85f)
				meter.SetTargetColor(NeedleMeterUIScript.ColorState.MAX);
			else
				meter.SetTargetColor(NeedleMeterUIScript.ColorState.NORMAL);
		}
		
		Refresh();
	}

	public static void SetKMPH(float speed) {
		if (kmph == null)
			return;
		int speedInt = (int)speed;
		string speedTxt = "";

		if (speedInt < 10) speedTxt = "00" + speedInt.ToString("F0");
		else if (speedInt < 100 && speedInt >= 10) speedTxt = "0" + speedInt.ToString("F0");
		else speedTxt = speedInt.ToString("F0");

		kmph.text = speedTxt;
	}
}
