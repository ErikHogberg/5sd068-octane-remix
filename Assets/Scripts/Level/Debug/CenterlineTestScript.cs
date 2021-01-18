using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class CenterlineTestScript : MonoBehaviour {

	public CenterlineScript Centerline;
	public Color LineColor = Color.blue;

	// TODO: draw 3d arrow for showing delta rotation at distance ahead
	// IDEA: float field for distance ahead, optional gameobject field for object to rotate

	public float DistanceAhead = 1;
	public float ArrowLength = 5;

	void OnDrawGizmos() {
		if (Centerline && Centerline.MainCenterline.LinePoints.Count > 1) {
			Gizmos.color = LineColor;
			Vector3 closestPos = Centerline.GetClosestPoint(transform.position, out int closestIndex, out int closestForkIndex);
			Gizmos.DrawLine(transform.position, Centerline.transform.TransformPoint(closestPos));

			Gizmos.color = Color.white;
			Vector3 delta = closestForkIndex < 0 ?
				Centerline.MainCenterline.LinePoints[closestIndex + 1] - Centerline.MainCenterline.LinePoints[closestIndex]
				:
				Centerline.Forks[closestForkIndex].LinePoints[closestIndex + 1] - Centerline.MainCenterline.LinePoints[closestIndex]
				;

			if (delta == Vector3.zero)
				return;

			Quaternion closestLineRot = Quaternion.LookRotation(delta, Vector3.forward);
			Gizmos.DrawLine(transform.position, transform.TransformPoint(closestLineRot * Vector3.forward * ArrowLength));

			var rots = Centerline.GetRotationDeltaAhead(closestPos, closestIndex, DistanceAhead);
			// * closestLineRot * Quaternion.Inverse(transform.rotation);
			if (rots.Any()) {
				Quaternion rot = rots.First().Item3 * closestLineRot * Quaternion.Inverse(transform.rotation);
				int index = rots.First().Item1;
				int forkIndex = rots.First().Item2;

				Gizmos.color = Color.green;
				if (index < 0) index = 0;
				// Vector3 cubeCenter = closestForkIndex < 0 ? Centerline.MainCenterline.LinePoints[index] : Centerline.Forks[forkIndex].LinePoints[index];
				Vector3 cubeCenter = forkIndex < 0 ? Centerline.MainCenterline.LinePoints[index] : Centerline.Forks[forkIndex].LinePoints[index];
				Gizmos.DrawCube(Centerline.transform.TransformPoint(cubeCenter), Vector3.one);
				Gizmos.DrawLine(transform.position, transform.TransformPoint(rot * Vector3.forward * ArrowLength));
			}

			var rots2 = Centerline.GetGreatestRotationDeltaAhead(closestPos, closestIndex, DistanceAhead);
			// * closestLineRot * Quaternion.Inverse(transform.rotation);
			if (rots2.Any()) {
				Quaternion rot = rots2.First().Item3 * closestLineRot * Quaternion.Inverse(transform.rotation);
				int indexAtEnd = rots2.First().Item1;
				int indexAtGreatestDelta = rots2.First().Item2;

				Gizmos.color = Color.cyan;
				if (indexAtEnd < 0) indexAtEnd = 0;
				if (indexAtGreatestDelta < 0) indexAtGreatestDelta = 0;
				// cubeCenter = closestForkIndex < 0 ? Centerline.MainCenterline.LinePoints[indexAtGreatestDelta] : Centerline.Forks[closestForkIndex].LinePoints[indexAtGreatestDelta];
				Gizmos.DrawCube(Centerline.transform.TransformPoint(Centerline.MainCenterline.LinePoints[indexAtGreatestDelta]), Vector3.one * .9f);
				Gizmos.DrawLine(transform.position, transform.TransformPoint(rot * Vector3.forward * ArrowLength));
			}

		}
	}
}
