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
			Vector3 closestPos = Centerline.GetClosestPoint(transform.position, out int closestIndex, out CenterlineScript.InternalCenterline closestFork);
			Gizmos.DrawLine(transform.position, Centerline.transform.TransformPoint(closestPos));

			Gizmos.color = Color.white;
			Vector3 delta = 
				closestFork.LinePoints[closestIndex + 1] - closestFork.LinePoints[closestIndex];
			// closestForkIndex < 0 ?
			// 	Centerline.MainCenterline.LinePoints[closestIndex + 1] - Centerline.MainCenterline.LinePoints[closestIndex]
			// 	:
			// 	Centerline.Forks[closestForkIndex].LinePoints[closestIndex + 1] - Centerline.MainCenterline.LinePoints[closestIndex]
			// 	;

			if (delta == Vector3.zero)
				return;

			Quaternion closestLineRot = Quaternion.LookRotation(delta, Vector3.forward);
			Gizmos.DrawLine(transform.position, transform.TransformPoint(closestLineRot * Vector3.forward * ArrowLength));

			foreach (var rotOut in CenterlineScript.GetRotationDeltaAhead(closestFork, DistanceAhead,closestIndex)) {
				Quaternion rot = rotOut.Item3 * closestLineRot * Quaternion.Inverse(transform.rotation);
				int index = rotOut.Item1;
				// int forkIndex = rotOut.Item2;
				CenterlineScript.InternalCenterline fork = rotOut.Item2;

				Gizmos.color = Color.green;
				if (index < 0) index = 0;
				// Vector3 cubeCenter = closestForkIndex < 0 ? Centerline.MainCenterline.LinePoints[index] : Centerline.Forks[forkIndex].LinePoints[index];
				// Vector3 cubeCenter = forkIndex < 0 ? Centerline.MainCenterline.LinePoints[index] : Centerline.Forks[forkIndex].LinePoints[index];
				Vector3 cubeCenter = fork.LinePoints[index];
				Gizmos.DrawCube(Centerline.transform.TransformPoint(cubeCenter), Vector3.one);
				Gizmos.DrawLine(transform.position, transform.TransformPoint(rot * Vector3.forward * ArrowLength));
			}

			foreach (var rotOut in CenterlineScript.GetGreatestRotationDeltasAhead(closestFork, DistanceAhead, closestIndex)) {
				Quaternion rot = rotOut.Item4 * closestLineRot * Quaternion.Inverse(transform.rotation);
				int indexAtEnd = rotOut.Item1;
				int indexAtGreatestDelta = rotOut.Item2;
				// int forkIndex = rotOut.Item3;
				CenterlineScript.InternalCenterline fork = rotOut.Item3;

				Gizmos.color = Color.cyan;
				if (indexAtEnd < 0) indexAtEnd = 0;
				if (indexAtGreatestDelta < 0) indexAtGreatestDelta = 0;
				// Vector3 cubeCenter = forkIndex < 0 ? Centerline.MainCenterline.LinePoints[indexAtGreatestDelta] : Centerline.Forks[forkIndex].LinePoints[indexAtGreatestDelta];
				Vector3 cubeCenter = fork.LinePoints[indexAtGreatestDelta];
				Gizmos.DrawCube(Centerline.transform.TransformPoint(cubeCenter), Vector3.one * .7f);
				Gizmos.DrawLine(transform.position, transform.TransformPoint(rot * Vector3.forward * ArrowLength));
			}

		}
	}
}
