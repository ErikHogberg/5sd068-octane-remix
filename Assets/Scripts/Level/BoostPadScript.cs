using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPadScript : MonoBehaviour {

	[Tooltip("Speed to either set or add")]
	public float Speed = 1f;
	[Tooltip("If the speed should be added instead of set")]
	public bool AddSpeed = false;
	[Tooltip("If the car should be rotated to face the direction of the boost pad")]
	public bool SetDirection = false;

	// TODO: option to either ignore or allow setting or adding speed values that would result in a lower speed
	// IDEA: if not allowed, interpret value as inverting direction
	// public bool AllowRemovingSpeed = false;

	private void OnTriggerEnter(Collider other) {
		var rb = other.attachedRigidbody; //.GetComponent<Rigidbody>();

		if (!rb)
			return;

		if (SetDirection){
			rb.MoveRotation(transform.rotation);
			rb.velocity = transform.forward * rb.velocity.magnitude;
		}

		if (AddSpeed)
			rb.velocity += Vector3.Normalize(rb.velocity) * Speed;
		else //if (rb.velocity.sqrMagnitude < Speed * Speed)
			rb.velocity = Vector3.Normalize(rb.velocity) * Speed;

	}

}
