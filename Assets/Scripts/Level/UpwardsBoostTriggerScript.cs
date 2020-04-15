using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpwardsBoostTriggerScript : MonoBehaviour {

	public float UpwardsForce = 100;
	public ForceMode Mode = ForceMode.Acceleration;

	// private void OnCollisionEnter(Collision other) {

	// }

	private void OnTriggerEnter(Collider other) {

		if (other.tag != "Player") {
			return;
		}

		Rigidbody rb = other.GetComponent<Rigidbody>();

		if (rb != null) {
			rb.AddForce(Vector3.up * UpwardsForce, Mode);
			print("rb up!");
		}

		print("up!");

	}

}
