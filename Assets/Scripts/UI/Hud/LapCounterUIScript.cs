using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LapCounterUIScript : MonoBehaviour, IObserver<int> {

    TMP_Text text;

	private void Start() {
        text = GetComponent<TMP_Text>();
		SteeringScript.MainInstance.LapCompletedObservers.Add(this);
	}

	// when car completes a lap
	public void Notify(int lapNumber) {
        text.text = lapNumber.ToString();
	}

}
