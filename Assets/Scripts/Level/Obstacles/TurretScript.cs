using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretScript : MonoBehaviour {

	public Transform HeadToTurn;

	[Min(0)]
	public float TurnSpeed = 1f;

	Transform target = null;

	Quaternion initRot;

	void Start() {
		initRot = HeadToTurn.rotation;
	}

	void Update() {

		if (target) {
			HeadToTurn.rotation = Quaternion.RotateTowards(
				HeadToTurn.rotation,
				Quaternion.LookRotation(target.position - HeadToTurn.position, transform.right),
				TurnSpeed * Time.deltaTime
			);
		} else {
			HeadToTurn.rotation = Quaternion.RotateTowards(
				HeadToTurn.rotation, 
				initRot, 
				TurnSpeed * Time.deltaTime
			);
		}

	}

	private void OnTriggerEnter(Collider other) {
		target = other.transform;
	}

	private void OnTriggerExit(Collider other) {
		target = null;
	}

}
