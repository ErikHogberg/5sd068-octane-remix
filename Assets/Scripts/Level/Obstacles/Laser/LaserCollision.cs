using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserCollision : MonoBehaviour
{
	private LaserWheelControls wheel;

	void Start() {
		wheel = transform.parent.GetComponent<LaserWheelControls>();
    }

	private void OnTriggerEnter(Collider other) {
		//Debug.Log("Laser hit! " + other.transform.name);
		TemperatureAndIntegrity handler = other.gameObject.GetComponent<TemperatureAndIntegrity>();
		//To not break code if collider is attached to a child of the car object's child or lower
		if (handler == null) {
			other.transform.parent.gameObject.GetComponent<TemperatureAndIntegrity>();
			if (handler == null) Debug.Log("LaserCollision: Unable to find TemperatureAndIntegrity of collided player. Is the collider more than one level down in the hierarchy?");
			else handler.LaserHit();
			return;
		}
		else handler.LaserHit();
	}
}
