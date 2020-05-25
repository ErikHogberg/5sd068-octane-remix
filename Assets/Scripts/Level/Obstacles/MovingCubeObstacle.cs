using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MovingCubeObstacle : MonoBehaviour {

	public float AttackSpeed;
	public float ResetSpeed;

	[Space]

	public Rigidbody Cube;
	[Tooltip("The position the block will head towards on trigger, goes towards car instead if not assigned")]
	public Transform OptionalTarget;

	Vector3 currentTarget;
	Vector3 initPos;
	bool attack = false;

	void Start() {
		initPos = Cube.position;
	}

	void Update() {
		if (attack) {
			Vector3 pos = Vector3.MoveTowards(Cube.position, currentTarget, AttackSpeed * Time.deltaTime);
			Cube.MovePosition(pos);
			if (Vector3.Distance(pos, Cube.position) == 0)
				attack = false;
		} else {
			Vector3 pos = Vector3.MoveTowards(Cube.position, initPos, ResetSpeed * Time.deltaTime);
			Cube.MovePosition(pos);
		}
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
