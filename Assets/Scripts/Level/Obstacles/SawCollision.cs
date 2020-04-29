using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SawCollision : MonoBehaviour
{
	private SawControls saw;

	void Start() {
		saw = transform.parent.GetComponent<SawControls>();
	}

	private void OnTriggerEnter(Collider other)
	{
		Debug.Log("Saw hit! " + other.transform.name);
		TemperatureAndIntegrity handler = other.gameObject.GetComponent<TemperatureAndIntegrity>();
		//To not break code if collider is attached to a child of the car object's child or lower
		if (handler == null) {
			other.transform.parent.gameObject.GetComponent<TemperatureAndIntegrity>();
			if (handler == null) Debug.Log("SawCollision: Unable to find TemperatureAndIntegrity of collided player. Is the collider more than one level down in the hierarchy?");
			else handler.SawHit();
			return;
		}
		else handler.SawHit();
	}
}
