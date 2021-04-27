﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RespawnTriggerScript : MonoBehaviour {

	public Color notificationColor = Color.blue;

	private void Respawn() {
		// if (!LevelPieceSuperClass.ResetToCurrentSegment()) {
		// 	SteeringScript.MainInstance?.CallResetObservers();
		// 	RemixEditorGoalPost.MoveCarToStart();
		// 	StartCountdownScript.StartPenaltyCountdownStatic(1.5f);
		// }

		SteeringScript.MainInstance?.Reset();
		UINotificationSystem.Notify("You fell off the track!", notificationColor, 1.5f);

	}

	private void OnTriggerEnter(Collider other) {
		if (!other.CompareTag("Player"))
			return;

		Respawn();
	}

	private void OnCollisionEnter(Collision other) {
		if (!other.gameObject.CompareTag("Player"))
			return;

		Respawn();
	}

}
