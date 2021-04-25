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

	public bool IgnoreEarlyEnd = false;
	public bool IncludeInactive = false;	

	public Vector3 ClosestPos;
	public CenterlineScript.InternalCenterline ClosestFork;
	public int ClosestIndex;

	void OnDrawGizmos() {
		if (Centerline && Centerline.MainCenterline.LinePoints.Count > 1) {
			Gizmos.color = LineColor;

			// query centerline for closest line point, meaning that the precision of the closest position to curve depends on the resolution of the line
			ClosestPos = Centerline.GetClosestPoint(transform.position, out ClosestIndex, out ClosestFork, IgnoreEarlyEnd, IncludeInactive);

			// draw the line between the test object and the closest line point
			Gizmos.DrawLine(transform.position, Centerline.transform.TransformPoint(ClosestPos));

			if (OtherTestObject != null) {
				// Gizmos.DrawSphere(ClosestPos, OtherClosestPosSize);
				// Gizmos.DrawSphere(OtherTestObject.ClosestPos, OtherClosestPosSize);
				Handles.Label(ClosestPos, "Start Line");
				Handles.Label(OtherTestObject.ClosestPos, "Finish Line");
			}

			Gizmos.color = Color.white;

			if (ClosestFork.LinePoints.Count < 2) {
				return;
			}

			Vector3 delta =
				ClosestFork.LinePoints[ClosestIndex + 1] - ClosestFork.LinePoints[ClosestIndex];

			if (delta == Vector3.zero)
				return;

			// get the tangent-like direction of the closest line point, which is the direction from that line point to the next one
			Quaternion closestLineRot = Quaternion.LookRotation(delta, Vector3.forward);
			// visualize the direction as a white line from the test object
			Gizmos.DrawLine(transform.position, transform.TransformPoint(closestLineRot * Vector3.forward * ArrowLength));

			// query and visualize the total change in direction over the defined distance ahead, for all forks within that distance from the closest line point along the line
			bool foundNoDeltas = true;
			Gizmos.color = Color.green;
			foreach (var rotOut in CenterlineScript.GetRotationDeltasAhead(ClosestFork, DistanceAhead, ClosestIndex, IgnoreEarlyEnd, IncludeInactive)) {
				foundNoDeltas = false;
				Quaternion rot = rotOut.Item3 * closestLineRot * Quaternion.Inverse(transform.rotation);
				int index = rotOut.Item1;
				CenterlineScript.InternalCenterline fork = rotOut.Item2;

				if (index < 0) index = 0;
				Vector3 cubeCenter = fork.LinePoints[index];
				// render a cube on the line to visualize how far ahead is being measured
				Gizmos.DrawCube(Centerline.transform.TransformPoint(cubeCenter), Vector3.one);
				// visualize the direction delta as a green line from the test object
				Gizmos.DrawLine(transform.position, transform.TransformPoint(rot * Vector3.forward * ArrowLength));
			}

			if (foundNoDeltas)
				// if the query returns no results, the cube is rendered at the center of the test object instead to signify this
				Gizmos.DrawCube(transform.position, Vector3.one);

			// query and visualize the greatest change in direction over the defined distance ahead, for all forks within that distance from the closest line point along the line
			Gizmos.color = Color.cyan;
			foundNoDeltas = true;
			foreach (var rotOut in CenterlineScript.GetGreatestRotationDeltasAhead(ClosestFork, DistanceAhead, ClosestIndex, IgnoreEarlyEnd, IncludeInactive)) {
				foundNoDeltas = false;
				Quaternion rot = rotOut.Item3 * closestLineRot * Quaternion.Inverse(transform.rotation);
				int indexAtGreatestDelta = rotOut.Item1;
				CenterlineScript.InternalCenterline fork = rotOut.Item2;

				if (indexAtGreatestDelta < 0) indexAtGreatestDelta = 0;
				Vector3 cubeCenter = fork.LinePoints[indexAtGreatestDelta];
				// render a cube on the line to visualize where on the line the greatest direction change was found within the distance ahead
				Gizmos.DrawCube(Centerline.transform.TransformPoint(cubeCenter), Vector3.one * .7f);
				// visualize the greatest direction delta(s) as a cyan line from the test object
				Gizmos.DrawLine(transform.position, transform.TransformPoint(rot * Vector3.forward * ArrowLength));
			}

			if (foundNoDeltas)
				// if the query returns no results, the cube is rendered at the center of the test object instead to signify this
				Gizmos.DrawCube(transform.position, Vector3.one * .7f);

			// goal post circumvention test
			if (SetActive || SetInactive) {
				if (OtherTestObject != null && OtherTestObject.ClosestFork != null) {

					// Gizmos.color = Color.green;
					// Gizmos.DrawSphere(OtherTestObject.ClosestFork.LinePoints.First(), OtherClosestPosSize);
					// Gizmos.DrawSphere(OtherTestObject.ClosestFork.LinePoints.Last(), OtherClosestPosSize);
					// Gizmos.color = Color.yellow;
					// Gizmos.DrawSphere(ClosestFork.LinePoints.First(), OtherClosestPosSize);
					// Gizmos.DrawSphere(ClosestFork.LinePoints.Last(), OtherClosestPosSize);

					// update the line to activate or deactivate the appropriate forks to prevent any path around the track from circumventing the goal post, using the closest points of the 2 test objects as start line and goal posts
					Centerline.SetReachableActive(ClosestFork, ClosestIndex, OtherTestObject.ClosestFork, OtherTestObject.ClosestIndex, SetActive, SetInactive);

					Gizmos.color = Color.white;
					if (ClosestFork.EarlyEndIndex >= 0) {
						// Debug.Log($"early end: {ClosestFork.EarlyEndIndex}");

						// visualize any "early ends" to lines, the points past which queries would ignore any more line points on this line/fork
						Handles.Label(ClosestFork.LinePoints[ClosestFork.EarlyEndIndex], $"early end: {ClosestFork.EarlyEndIndex}");
						Gizmos.DrawSphere(ClosestFork.LinePoints[ClosestFork.EarlyEndIndex], OtherClosestPosSize);
					}
					// if (OtherTestObject.ClosestFork.EarlyEndIndex >= 0)
					// 	Gizmos.DrawSphere(OtherTestObject.ClosestFork.LinePoints[OtherTestObject.ClosestFork.EarlyEndIndex], OtherClosestPosSize);


				} else {
					// update the line to activate or deactivate the appropriate forks to prevent any path around the track from circumventing the goal post, using the closest point of this test object as both start line and goal post
					Centerline.SetReachableActive(ClosestFork, ClosestIndex, ClosestFork, ClosestIndex, SetActive, SetInactive);
					Gizmos.color = Color.white;
					if (ClosestFork.EarlyEndIndex >= 0) {
						// visualize any "early ends" to lines, the points past which queries would ignore any more line points on this line/fork
						Handles.Label(ClosestFork.LinePoints[ClosestFork.EarlyEndIndex] + Vector3.right * 30f, $"early end: {ClosestFork.EarlyEndIndex}");
						Gizmos.DrawSphere(ClosestFork.LinePoints[ClosestFork.EarlyEndIndex], OtherClosestPosSize);
					}

				}

				// force repainting of the editor scene view 
				SceneView.RepaintAll();
			}

		}
	}
}
