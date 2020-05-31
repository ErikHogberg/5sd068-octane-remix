using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecelerationZoneScript : MonoBehaviour {

	public float DecelerationSpeed;
	public float MinSpeed;

	private void OnTriggerEnter(Collider other) {
		UINotificationSystem.Notify("Slow Zone!",Color.blue, 1);
	}

	private void OnTriggerStay(Collider other) {
		var rb = other.attachedRigidbody;

		if (!rb || rb.velocity.sqrMagnitude < MinSpeed * MinSpeed)
			return;

		rb.velocity = Vector3.MoveTowards(
			rb.velocity,
			Vector3.Normalize(rb.velocity) * MinSpeed,
			DecelerationSpeed * Time.deltaTime
		);

	}

}
