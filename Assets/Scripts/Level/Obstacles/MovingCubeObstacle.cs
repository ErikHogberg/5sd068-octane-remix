using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MovingCubeObstacle : MonoBehaviour {

	public float AttackForce;
	public float ResetForce;


	[Space]

	public Rigidbody Cube;
	[Tooltip("The position the block will head towards on trigger, goes towards car instead if not assigned")]
	public Transform OptionalTarget;

	Vector3 currentTarget;
	bool attack = false;

	void Start() {

	}

	void Update() {

	}

	private void OnTriggerEnter(Collider other) {
		attack = true;
		if (OptionalTarget) {
			currentTarget = OptionalTarget.position;
		} else {
			currentTarget = other.transform.position;
		}
	}

}
