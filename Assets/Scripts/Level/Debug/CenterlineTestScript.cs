using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CenterlineTestScript : MonoBehaviour {

	public CenterlineScript Centerline;
	public Color LineColor = Color.blue;

	// TODO: draw 3d arrow for showing delta rotation at distance ahead
	// IDEA: float field for distance ahead, optional gameobject field for object to rotate

	public float DistanceAhead = 1;
	public float ArrowLength = 5;

	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void Update() {

	}

	void OnDrawGizmos() {
		if (Centerline && Centerline.LinePoints.Count > 1) {
			Gizmos.color = LineColor;
			Vector3 closestPos = Centerline.GetClosestPoint(transform.position, out int closestIndex);
			Gizmos.DrawLine(transform.position, closestPos);

			Gizmos.color = Color.white;
			Quaternion closestLineRot = Quaternion.LookRotation(Centerline.LinePoints[closestIndex + 1] - Centerline.LinePoints[closestIndex], Vector3.forward);
			Gizmos.DrawLine(transform.position, transform.TransformPoint(closestLineRot * Vector3.forward * ArrowLength));

			Quaternion rot = Centerline.GetRotationDeltaAhead(closestPos, closestIndex, DistanceAhead, out int index)
				* closestLineRot * Quaternion.Inverse(transform.rotation);

			Gizmos.color = Color.green;
			if (index < 0) index = 0;
			Gizmos.DrawCube(Centerline.LinePoints[index], Vector3.one);
			Gizmos.DrawLine(transform.position, transform.TransformPoint(rot * Vector3.forward * ArrowLength));

			rot = Centerline.GetGreatestRotationDeltaAhead(closestPos, closestIndex, DistanceAhead, out int indexAtEnd, out int indexAtGreatestDelta)
				* closestLineRot * Quaternion.Inverse(transform.rotation);

			Gizmos.color = Color.cyan;
			if (indexAtEnd < 0) indexAtEnd = 0;
			if (indexAtGreatestDelta < 0) indexAtGreatestDelta = 0;
			Gizmos.DrawCube(Centerline.LinePoints[indexAtGreatestDelta], Vector3.one);
			Gizmos.DrawLine(transform.position, transform.TransformPoint(rot * Vector3.forward * ArrowLength));

		}
	}
}
