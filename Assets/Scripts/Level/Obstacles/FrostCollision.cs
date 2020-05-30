﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostCollision : MonoBehaviour {

	private void OnTriggerEnter(Collider other) {
		Debug.Log("Flame hit! " + other.transform.name);
		TemperatureAndIntegrity handler = other.gameObject.GetComponent<TemperatureAndIntegrity>();
		//To not break code if collider is attached to a child of the car object's child or lower
		if (handler == null) {
			other.transform.parent.gameObject.GetComponent<TemperatureAndIntegrity>();
			if (handler == null) Debug.Log("FrostCollision: Unable to find TemperatureAndIntegrity of collided player. Is the collider more than one level down in the hierarchy?");
			else handler.FrostHit();
			return;
		} else handler.FrostHit();
	}

	private void OnTriggerStay(Collider other) {
		if (other.TryGetComponent<TemperatureAndIntegrity>(out TemperatureAndIntegrity car)) {
			car.FrostHit();
		}
	}


}
