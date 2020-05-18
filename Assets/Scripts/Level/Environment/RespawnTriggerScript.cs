using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RespawnTriggerScript : MonoBehaviour {

	private void OnTriggerEnter(Collider other) {
		LevelPieceSuperClass.ResetToCurrentSegment();
		UINotificationSystem.Notify("You fell off the track!", Color.blue, 1.5f);
	}

	private void OnCollisionEnter(Collision other) {
		LevelPieceSuperClass.ResetToCurrentSegment();
		UINotificationSystem.Notify("You fell off the track!", Color.blue, 1.5f);
	}

}
