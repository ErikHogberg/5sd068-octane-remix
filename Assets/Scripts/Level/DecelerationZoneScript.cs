using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecelerationZoneScript : MonoBehaviour {

	public float DecelerationSpeed = 100f;
	public float MinSpeed = 30f;

	// private void OnTriggerEnter(Collider other) {
	// 	UINotificationSystem.Notify("Slow Zone!",Color.blue, 1);
	// }

	private void SlowDownRigidbody(Rigidbody rb) {
		if (!rb || rb.velocity.sqrMagnitude < MinSpeed * MinSpeed)
			return;

		rb.velocity = Vector3.MoveTowards(
			rb.velocity,
			Vector3.Normalize(rb.velocity) * MinSpeed,
			DecelerationSpeed * Time.deltaTime
		);
	}

	private void OnTriggerStay(Collider other) {
		SlowDownRigidbody(other.attachedRigidbody);
	}

	private void OnCollisionStay(Collision other) {
		SlowDownRigidbody(other.rigidbody);
	}

}
