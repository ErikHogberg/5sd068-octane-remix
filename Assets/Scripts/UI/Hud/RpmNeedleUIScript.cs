using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RpmNeedleUIScript : MonoBehaviour {

	public float MaxRpm;

	private NeedleMeterUIScript meter;
	private TMP_Text kmph;

	void Awake() {
		meter = GetComponent<NeedleMeterUIScript>();
		kmph = transform.parent.transform.Find("TextRPM").GetComponent<TMP_Text>();
	}

	private void Update() {

		if (!SteeringScript.MainInstance)
			return;

		float rpm = 0;
		foreach (var item in SteeringScript.MainInstance.FrontWheelColliders.Concat(SteeringScript.MainInstance.RearWheelColliders)) {
			float wheelRpm =  Mathf.Abs(item.rpm);
			if (rpm < wheelRpm)
				rpm = wheelRpm;
		}

		SetRpm(rpm);
		SetBarPercentage(rpm / MaxRpm, false);
		Refresh();

	}

	public void Refresh() {
		if (meter == null)
			return;

		meter.UpdateBarPercentage();
		meter.ApplyColor();
	}

	public void SetBarPercentage(float percentage, bool isBoosting) {
		if (meter == null)
			return;

		meter.SetTargetPercent(percentage);
		if (isBoosting)
			meter.SetTargetColor(NeedleMeterUIScript.ColorState.BOOST);
		else {
			if (percentage > 0.97f)
				meter.SetTargetColor(NeedleMeterUIScript.ColorState.MAX);
			else
				meter.SetTargetColor(NeedleMeterUIScript.ColorState.NORMAL);
		}

		Refresh();
	}

	public void SetRpm(float speed) {
		if (!kmph)
			return;

		kmph.text = speed.ToString("F0");
	}
}
