using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelUpdater : MonoBehaviour {

	[Serializable]
	public class WheelUpdaterPair {
		public WheelCollider collider;
		public Transform model;
	}

	// public (WheelCollider, Transform)[] Wheels;
	public WheelUpdaterPair[] Wheels;

	void FixedUpdate() {
		foreach (WheelUpdaterPair wheel in Wheels) {
			wheel.collider.GetWorldPose(out Vector3 pos, out Quaternion rot);
			wheel.model.position = pos;
			wheel.model.rotation = rot;
		}
	}

}
