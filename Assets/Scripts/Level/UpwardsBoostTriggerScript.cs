using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpwardsBoostTriggerScript : MonoBehaviour {

	public float UpwardsForce = 100;
	public ForceMode Mode = ForceMode.Acceleration;

	public bool CorrectFacing = false;


	private void OnTriggerEnter(Collider other) {

		// TODO: dont collide with fliptrigger

		other.attachedRigidbody.AddForce(Vector3.up * UpwardsForce, Mode);

		if (CorrectFacing) {
			// TODO: set car direction (only along world x) to this objects direction
		}

		// print("up! " + other.name);

	}

}
