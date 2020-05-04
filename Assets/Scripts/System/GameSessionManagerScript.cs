using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSessionManagerScript : MonoBehaviour {

	public int MaxLaps = 3;

	private bool gameOver = false;


	private void EndGame(){
		gameOver = true;
	}

	public void NotifyLap(SteeringScript car) {
		
		if (!gameOver && car.LapsCompleted >= 3) {
			// TODO: still count laps and decide finish order for other cars than winner in 3+ multiplayer
			EndGame();
		}
	}

}
