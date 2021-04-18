using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class CenterlineTestScript : MonoBehaviour {

	public CenterlineScript Centerline;
	public Color LineColor = Color.blue;

	// TODO: draw 3d arrow for showing delta rotation at distance ahead
	// IDEA: float field for distance ahead, optional gameobject field for object to rotate

	public float DistanceAhead = 1;
	public float ArrowLength = 5;

	public bool SetActive = false;
	public bool SetInactive = false;

	public CenterlineTestScript OtherTestObject;
	public float OtherClosestPosSize = 1f;

	public Vector3 ClosestPos;
	public CenterlineScript.InternalCenterline ClosestFork;
	public int ClosestIndex;

	void OnDrawGizmos() {
		if (Centerline && Centerline.MainCenterline.LinePoints.Count > 1) {
			Gizmos.color = LineColor;
			ClosestPos = Centerline.GetClosestPoint(transform.position, out ClosestIndex, out ClosestFork);
			Gizmos.DrawLine(transform.position, Centerline.transform.TransformPoint(ClosestPos));
			if (OtherTestObject != null)
				Gizmos.DrawSphere(ClosestPos, OtherClosestPosSize);

			Gizmos.color = Color.white;

			if (ClosestFork.LinePoints.Count < 2) {
				return;
			}

			Vector3 delta =
				ClosestFork.LinePoints[ClosestIndex + 1] - ClosestFork.LinePoints[ClosestIndex];

			if (delta == Vector3.zero)
				return;

			Quaternion closestLineRot = Quaternion.LookRotation(delta, Vector3.forward);
			Gizmos.DrawLine(transform.position, transform.TransformPoint(closestLineRot * Vector3.forward * ArrowLength));

			bool foundNoDeltas = true;
			Gizmos.color = Color.green;
			foreach (var rotOut in CenterlineScript.GetRotationDeltasAhead(ClosestFork, DistanceAhead, ClosestIndex)) {
				foundNoDeltas = false;
				Quaternion rot = rotOut.Item3 * closestLineRot * Quaternion.Inverse(transform.rotation);
				int index = rotOut.Item1;
				CenterlineScript.InternalCenterline fork = rotOut.Item2;

				if (index < 0) index = 0;
				Vector3 cubeCenter = fork.LinePoints[index];
				Gizmos.DrawCube(Centerline.transform.TransformPoint(cubeCenter), Vector3.one);
				Gizmos.DrawLine(transform.position, transform.TransformPoint(rot * Vector3.forward * ArrowLength));
			}

			if (foundNoDeltas)
				Gizmos.DrawCube(transform.position, Vector3.one);

			Gizmos.color = Color.cyan;
			foundNoDeltas = true;
			foreach (var rotOut in CenterlineScript.GetGreatestRotationDeltasAhead(ClosestFork, DistanceAhead, ClosestIndex)) {
				foundNoDeltas = false;
				Quaternion rot = rotOut.Item3 * closestLineRot * Quaternion.Inverse(transform.rotation);
				int indexAtGreatestDelta = rotOut.Item1;
				CenterlineScript.InternalCenterline fork = rotOut.Item2;

				if (indexAtGreatestDelta < 0) indexAtGreatestDelta = 0;
				Vector3 cubeCenter = fork.LinePoints[indexAtGreatestDelta];
				Gizmos.DrawCube(Centerline.transform.TransformPoint(cubeCenter), Vector3.one * .7f);
				Gizmos.DrawLine(transform.position, transform.TransformPoint(rot * Vector3.forward * ArrowLength));
			}

			if (foundNoDeltas)
				Gizmos.DrawCube(transform.position, Vector3.one * .7f);

			if (SetActive || SetInactive) {
				if (OtherTestObject != null && OtherTestObject.ClosestFork != null) {
					Gizmos.DrawSphere(OtherTestObject.ClosestPos, OtherClosestPosSize);
					
					Gizmos.color = Color.green;
					Gizmos.DrawSphere(OtherTestObject.ClosestFork.LinePoints.First(), OtherClosestPosSize);
					Gizmos.DrawSphere(OtherTestObject.ClosestFork.LinePoints.Last(), OtherClosestPosSize);
					Gizmos.color = Color.yellow;
					Gizmos.DrawSphere(ClosestFork.LinePoints.First(), OtherClosestPosSize);
					Gizmos.DrawSphere(ClosestFork.LinePoints.Last(), OtherClosestPosSize);
					Centerline.SetReachableActive(ClosestFork, ClosestIndex, OtherTestObject.ClosestFork, OtherTestObject.ClosestIndex, SetActive, SetInactive);

				} else {

					Centerline.SetReachableActive(ClosestFork, ClosestIndex, ClosestFork, ClosestIndex, SetActive, SetInactive);
				}
				SceneView.RepaintAll();
			}

		}
	}
}
