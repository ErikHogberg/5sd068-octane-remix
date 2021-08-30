using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockCollision : MonoBehaviour {

	public bool LimitVelocityOnHit = false;
	public float VelocityLimit = 100f;

	private void OnCollisionEnter(Collision other) {
		if (!other.gameObject.CompareTag("Player"))
			return;

		// NOTE: should work as-is with multiplayer too
		if (other.gameObject.TryGetComponent<TemperatureAndIntegrity>(out TemperatureAndIntegrity carTemp)) {
			var contact0 = other.GetContact(0);
			// IDEA: send contact(s?) instead of pos and normal
			carTemp.RockHit(other.relativeVelocity.sqrMagnitude, contact0);
		}

		if (LimitVelocityOnHit
		&& other.gameObject.TryGetComponent<Rigidbody>(out Rigidbody carRb)
		&& carRb.velocity.sqrMagnitude > VelocityLimit * VelocityLimit) {
			carRb.velocity = carRb.velocity.normalized * VelocityLimit;
		}


	}


}
