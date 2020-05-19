using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPadScript : MonoBehaviour {

	public enum AngularVelocityMode {
		None, // skip setting angular velocity
		Zero, // set angular velocity to 0
		TowardsFacing, // set angular velocity relative to world, but with world forward rotated in projected direction of boost pad
		World,// set angular velocity relative to world
		Local // set angular velocity relative to car direction
	}

	[Tooltip("Speed to either set or add")]
	public float Speed = 1f;
	[Tooltip("If the speed should be added instead of set")]
	public bool AddSpeed = false;
	[Tooltip("If the car should be rotated to face the direction of the boost pad")]
	public bool SetDirection = false;
	[Tooltip("If the speed is added in the direction of the boost pad instead of the car")]
	public bool SetSpeedInDirection = false;

	[Space]
	[Tooltip("If the angular velocity of the car should be set when touching the boost pad. Has multiple modes of applying angular velocity")]
	public AngularVelocityMode SetAngularVelocity = AngularVelocityMode.None;
	[Tooltip("What angular velocity the car should be set to when touching the boost pad. Value is used differently depending on mode")]
	public Vector3 AngularVelocity;

	[Space]
	[Tooltip("Assign an object here to use its forward direction instead of the forward direction of this object. The object position can be anywhere")]
	public Transform OptionalDirectionOverride;


	// TODO: option to either ignore or allow setting or adding speed values that would result in a lower speed
	// IDEA: if not allowed, interpret value as inverting direction
	// public bool AllowRemovingSpeed = false;

	private void OnTriggerEnter(Collider other) {
		var rb = other.attachedRigidbody;

		if (!rb)
			return;

		Transform directionTransform = transform;
		if (OptionalDirectionOverride)
			directionTransform = OptionalDirectionOverride;

		if (SetDirection) {
			rb.MoveRotation(directionTransform.rotation);
			// rb.velocity = transform.forward * rb.velocity.magnitude;
		}

		// if (AddSpeed && Speed == 0)
		// 	return;

		if (SetSpeedInDirection) {
			if (!AddSpeed)
				rb.velocity = Vector3.zero;
			rb.AddForce(directionTransform.forward * Speed, ForceMode.VelocityChange);
		} else {
			if (AddSpeed)
				rb.velocity += Vector3.Normalize(rb.velocity) * Speed;
			else //if (rb.velocity.sqrMagnitude < Speed * Speed)
				rb.velocity = Vector3.Normalize(rb.velocity) * Speed;
		}

		// if (SetAngularVelocity)
		// 	rb.angularVelocity = (directionTransform.forward - rb.transform.forward) * AngularVelocity;

		switch (SetAngularVelocity) {
			case AngularVelocityMode.None:
				break;
			case AngularVelocityMode.Zero:
				rb.angularVelocity = Vector3.zero;
				break;
			case AngularVelocityMode.TowardsFacing:

				var dir = Vector3.Project(directionTransform.forward, Vector3.up).normalized;
				var quat = Quaternion.FromToRotation(Vector3.forward, dir);
				rb.angularVelocity = quat * AngularVelocity;
				// rb.angularVelocity = AngularVelocity;// * (directionTransform.forward - rb.transform.forward);
				break;
			case AngularVelocityMode.World:
				rb.angularVelocity = AngularVelocity;
				break;
			case AngularVelocityMode.Local:
				rb.angularVelocity = rb.transform.localToWorldMatrix * AngularVelocity;
				break;
		}

	}

}
