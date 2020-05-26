using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeBlockScript : ExplodeComponent {

	Rigidbody rb;
	Vector3 initPos;
	Quaternion initRot;

	[Tooltip("How strong the explosion is")]
	public float ExplosionForce = 1f;

	private void Start() {
		rb = GetComponent<Rigidbody>();
		initPos = rb.position;
		initRot = rb.rotation;
	}

	public override void Explode() {
		rb.isKinematic = false;
		rb.AddForce(
			Vector3.up * ExplosionForce
		);
	}

	public override void UndoExplode() {
		rb.isKinematic = true;
		rb.position = initPos;
		rb.rotation = initRot;
	}
}
