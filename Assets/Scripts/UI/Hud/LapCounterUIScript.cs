using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LapCounterUIScript : MonoBehaviour, IObserver<int> {

	TMP_Text text;

	private static LapCounterUIScript mainInstance;

	private void Awake() {
		mainInstance = this;
	}

	public static void SetCar() {
		if (mainInstance)
			SteeringScript.MainInstance.LapCompletedObservers.Add(mainInstance);
	}

	bool init = false;
	private void Start() {
		// SteeringScript.MainInstance.LapCompletedObservers.Add(this);
		// Debug.LogWarning("lap steering: " + SteeringScript.MainInstance.gameObject.name);

		text = GetComponent<TMP_Text>();
	}

	// private void Update() {
	// 	if (init) {
	// 		return;
	// 	}

	// 	SteeringScript.MainInstance.LapCompletedObservers.Add(this);
	// 	init = true;		
	// }

	// when car completes a lap
	public void Notify(int lapNumber) {
		text.text = lapNumber.ToString();
	}

}
