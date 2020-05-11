using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockCollision : MonoBehaviour
{

	private void OnCollisionEnter(Collision other) {
		// NOTE: should work as-is with multiplayer too
		var car = other.gameObject.GetComponent<TemperatureAndIntegrity>();
		if (!car)
			return;

		car.RockHit();
	}


}
