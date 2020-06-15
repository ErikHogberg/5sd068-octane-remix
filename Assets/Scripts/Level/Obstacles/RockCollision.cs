using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockCollision : MonoBehaviour {

	private void OnCollisionEnter(Collision other) {
		if (!other.gameObject.CompareTag("Player"))
			return;
		
		// NOTE: should work as-is with multiplayer too
		if (other.gameObject.TryGetComponent<TemperatureAndIntegrity>(out TemperatureAndIntegrity car))
			car.RockHit(other.relativeVelocity.sqrMagnitude);

	}


}
