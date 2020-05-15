using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverPanel : MonoBehaviour, IObserver<int> {

	// NOTE: temporary, max laps setting will probably be moved somewhere else once other scripts start using it
	public int MaxLaps = 3;

	void Start() {
		SteeringScript.MainInstance.LapCompletedObservers.Add(this);
		gameObject.SetActive(false);
	}

	public void Notify(int lapsCompleted) {
		if (lapsCompleted >= MaxLaps) {
			// TODO: disable car controls
			gameObject.SetActive(true);
		}
	}

}
