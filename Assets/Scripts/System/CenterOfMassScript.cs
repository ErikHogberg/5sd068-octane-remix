using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CenterOfMassScript : MonoBehaviour {

	public Transform CustomCenterOfMass;

	void Start() {
		var rb = GetComponent<Rigidbody>();

		if (CustomCenterOfMass != null) {
			rb.centerOfMass = CustomCenterOfMass.position - transform.position;
		}
	}

}
