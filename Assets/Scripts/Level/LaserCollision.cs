using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserCollision : MonoBehaviour
{
	private LaserWheelControls wheel;

	void Start() {
		wheel = transform.parent.GetComponent<LaserWheelControls>();

    }

	private void OnTriggerEnter(Collider other)
    {
		wheel.LogHit();
		
	}
}
