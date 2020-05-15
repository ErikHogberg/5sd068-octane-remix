using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericLookAtScript : MonoBehaviour, IFollowScript {

	public Transform Target;
	public Vector2 AngleOffset;

	void Update() {
		transform.LookAt(Target);
		transform.Rotate(Vector2.up, AngleOffset.x, Space.Self);
		transform.Rotate(Vector2.right, AngleOffset.y, Space.Self);
	}

	public void SetFollowTarget(Transform target) {
		Target = target;
	}
}
