﻿using UnityEngine;

public class GenericLookAtScript : MonoBehaviour, IFollowScript {

	public Transform Target;
	public Vector2 AngleOffset;

	private void Start() {
		Target = Target ?? SteeringScript.MainInstance?.transform;
	}

	void Update() {
		transform.LookAt(Target);
		transform.Rotate(Vector2.up, AngleOffset.x, Space.Self);
		transform.Rotate(Vector2.right, AngleOffset.y, Space.Self);
	}

	public void SetFollowTarget(Transform target) {
		Target = target;
	}
}
