using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MovingCubeTriggerObstacle : MonoBehaviour {

	[Min(0.01f)]
	public float AttackTime;
	[Min(0.01f)]
	public float ResetTime;

	public AnimationCurve MoveCurve = AnimationCurve.Linear(0, 0, 1, 1);

	[Space]

	public GameObject Cube;
	[Tooltip("The position the block will head towards on trigger, goes towards car instead if not assigned")]
	public Transform OptionalTarget;

	Vector3 currentTarget;
	Vector3 initPos;
	bool attack = false;
	float timer = -1;

	void Start() {
		initPos = Cube.transform.position;
	}

	void Update() {
		if (!attack && timer < 0) {
			return;
		}
		timer -= Time.deltaTime;
		
		float percentage = 0;
		if (attack) {
			percentage = timer / AttackTime;
			percentage = MoveCurve.Evaluate(1 - percentage);

			if (timer < 0) {
				attack = false;
				timer = ResetTime;
				percentage = 1;
			}
		} else {
			percentage = timer / ResetTime;
			percentage = MoveCurve.Evaluate(percentage);
			if (timer < 0) {
				percentage = 0;
			}
		}
		Vector3 pos = Vector3.Lerp(initPos, currentTarget, percentage);
		Cube.transform.position = pos;
	}

	private void OnTriggerEnter(Collider other) {
		if (timer > 0) {
			return;
		}

		attack = true;
		timer = AttackTime;
		if (OptionalTarget) {
			currentTarget = OptionalTarget.position;
		} else {
			currentTarget = other.transform.position;

		}
	}

}
