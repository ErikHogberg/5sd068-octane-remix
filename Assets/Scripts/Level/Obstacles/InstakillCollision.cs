using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstakillCollision : MonoBehaviour {

	public bool OnCollision = true;
	public bool OnTrigger = false;

	private void OnCollisionEnter(Collision other) {
		if (!other.gameObject.CompareTag("Player"))
			return;
		
		if (OnCollision && other.gameObject.TryGetComponent<TemperatureAndIntegrity>(out TemperatureAndIntegrity car))
			car.Instakill();
	}

	private void OnTriggerEnter(Collider other) {
		if (!other.gameObject.CompareTag("Player"))
			return;
		
		if (OnTrigger && other.TryGetComponent<TemperatureAndIntegrity>(out TemperatureAndIntegrity car))
			car.Instakill();
	}
}
