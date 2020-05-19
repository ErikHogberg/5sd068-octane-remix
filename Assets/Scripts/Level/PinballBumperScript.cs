using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinballBumperScript : MonoBehaviour {

	public Transform ReflectionDirection;

	public bool OnCollision = false;
	public bool OnTrigger = false;

	public bool LimitVelocity = false;
	public float VelocityLimit = 1f;

	public void Bounce(Rigidbody rb) {
		// TODO: option to bounce off relative to bumper center instead of plane

		rb.velocity = Vector3.Reflect(rb.velocity, ReflectionDirection.forward);

		if (LimitVelocity && rb.velocity.magnitude > VelocityLimit) 
			rb.velocity = rb.velocity.normalized * VelocityLimit;
		
	}

	private void OnCollisionEnter(Collision other) {
		if (!OnCollision || !other.rigidbody)
			return;

		Bounce(other.rigidbody);
	}

	private void OnTriggerEnter(Collider other) {
		if (!OnTrigger || !other.attachedRigidbody)
			return;

		Bounce(other.attachedRigidbody);
	}

}
