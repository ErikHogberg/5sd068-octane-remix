using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(BarUIScript))]
public class TemperatureUIScript : MonoBehaviour
{

	// TODO: List of instances, ordered by player index, for 2+ player split screen
	public static TemperatureUIScript MainInstance;

	private BarUIScript bar;
	public TMP_Text temperatureText;

	void Awake() {
		MainInstance = this;
		bar = GetComponent<BarUIScript>();
	}

	void OnDestroy() {
		MainInstance = null;
	}

	public void SetTempPercentage(float percentage, float temp)
	{
		if (bar == null || temperatureText == null) {
			Debug.Log("TemperatureUIScript: " + transform.parent.name + " is missing either a BarUIScript component or an assigned TMP_Text");
			return;
		}

		bar.SetBarPercentage(percentage);
		temperatureText.text = temp.ToString("F0") + "°C";

		/*if (color.HasValue) 
			bar.SetColor(color.Value);*/

	}
}
