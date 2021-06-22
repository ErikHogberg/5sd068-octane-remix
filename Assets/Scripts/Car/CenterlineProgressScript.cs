using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CenterlineProgressScript : MonoBehaviour {

	// Cheat mitigation
	int lastValidIndex = -1;
	CenterlineScript.InternalCenterline lastValidLine = null;

	int furthestValidIndex = -1;
	CenterlineScript.InternalCenterline furthestValidLine = null;


	private Vector3 resetPos = Vector3.zero;

	public float CheatMitigationSearchDistance = 10;
	public float CheatMitigationLookBehindDistance = 1;

	private CenterlineScript.InternalCenterline lastForkParent = null;
	public float CheatCheckPerSec = 5f;

	private float timer = -1;

	public bool CheckInUpdate = false;

	public UnityEvent ResetEvent;
	public UnityEvent LapEvent;

	void Update() {

		if (!CheckInUpdate)
			return;

		timer -= Time.deltaTime;
		if (timer < 0) {
			timer += 1f / CheatCheckPerSec;
			if (!QueryProgress(out bool lapCompleted)) {
				ResetEvent.Invoke();
				if (lapCompleted)
					LapEvent.Invoke();
			}
		}

	}

#if UNITY_EDITOR
	void OnDrawGizmos() {
		if (CenterlineScript.IsInitialized) {
			Gizmos.color = Color.magenta;

			Vector3 closestPos = CenterlineScript.GetClosestPointStatic(transform.position, out int index, out var fork, out float distance);

			// draw the line between the test object and the closest line point
			Gizmos.DrawLine(transform.position, closestPos);

			if (lastValidLine != null)
				Gizmos.DrawCube(CenterlineScript.MainInstanceTransform.TransformPoint(lastValidLine.LinePoints[lastValidIndex]), Vector3.one);

			Gizmos.color = Color.green;
			Gizmos.DrawLine(transform.position, resetPos);
			Gizmos.DrawCube(resetPos, Vector3.one);

			foreach ((int endIndex, var line, _) in CenterlineScript.GetRotationDeltasAhead(fork, CoDriverUIScript.CheckAheadDistanceStatic, index)) {
				Gizmos.color = Color.cyan;
				Gizmos.DrawCube(CenterlineScript.MainInstanceTransform.TransformPoint(line.LinePoints[endIndex]), Vector3.one);

			}

		}
	}
#endif

	// Moves and rotates the object the component is attached to to the last valid point on the centerline
	// returns true if reset completed successfully. reset will always fail if there is no centerline.
	public bool ResetTransform() {
		if (CenterlineScript.IsInitialized && lastValidIndex >= 0) {
			transform.position =
				resetPos;
			// CenterlineScript.GetClosestPointStatic(transform.position, out int index, out var line, out float distance);


			// IDEA: instead of placing the car in the air, place the car close to the ground by raycasting below the closest position on the line

			// if (line.LinePoints.Count > 2 && index < line.LinePoints.Count - 1)
			if (lastValidLine.LinePoints.Count > 2 && lastValidIndex < lastValidLine.LinePoints.Count - 1)
				transform.rotation = Quaternion.LookRotation(lastValidLine.LinePoints[lastValidIndex + 1] - lastValidLine.LinePoints[lastValidIndex], Vector3.up);

			return true;
		}

		return false;
	}


	// returns true if progress is allowed. progress is always allowed if there is no centerline
	public bool QueryProgress(out bool lapCompleted) {
		lapCompleted = false;
		// TODO: check if finish line was passed
		// TODO: don't trigger lap completion if backtracking progress marker over finish line, which is possible when finish line is in look behind range of a fork start
		// TODO: teleport car to start line if start line is separate from finish line
		// IDEA: query centerline if allowed to finish lap when touching goal post object, increment lap anyway if skipping finish line and leaving allowed lap finish range (by query returning false next frame before getting a goal post collision signal)

		if (!CenterlineScript.IsInitialized)
			return true;

		CenterlineScript.InternalCenterline preLine = lastValidLine;
		int preIndex = lastValidIndex;

		float resetDistance = CenterlineScript.ResetDistanceStatic;
		if (lastValidIndex < 0) {
			resetPos = CenterlineScript.GetClosestPointStatic(transform.position, out int index, out var line, out float distance);
			lastValidIndex = index;
			lastValidLine = line;
			if (resetDistance > 0 && distance > resetDistance) {
				// ResetTransform();
				return false;
			}
		} else {

			(int checkStartIndex, float distanceBehindFoundSqr) = CenterlineScript.GetEarliestForkStartBehind(lastValidLine, CheatMitigationLookBehindDistance, lastValidIndex);

			float distance;
			(int, CenterlineScript.InternalCenterline) linePoint;

			if (checkStartIndex < 0
			&& lastForkParent != null
			&& lastForkParent != lastValidLine
			) {
				resetPos = CenterlineScript.GetClosestPointWithinRangeToIndexStatic(
					transform.position,
					lastForkParent,
					CheatMitigationSearchDistance * CheatMitigationSearchDistance,
					out distance,
					out linePoint,
					lastValidLine.StartIndex
				);
			} else {
				if (checkStartIndex < 0) {
					checkStartIndex = lastValidIndex;
				}
				resetPos = CenterlineScript.GetClosestPointWithinRangeToIndexStatic(
					transform.position,
					lastValidLine,
					CheatMitigationSearchDistance * CheatMitigationSearchDistance + distanceBehindFoundSqr,
					out distance,
					out linePoint,
					checkStartIndex
				);
			}


			lastValidIndex = linePoint.Item1;

			if (lastValidLine != linePoint.Item2) {
				if (lastValidLine.Forks.Contains(linePoint.Item2))
					lastForkParent = lastValidLine;
				else
					lastForkParent = null;
			}
			lastValidLine = linePoint.Item2;
			if (resetDistance > 0 && distance > resetDistance) {
				// ResetTransform();
				return false;
			}

			CenterlineScript.InternalCenterline postLine = lastValidLine;
			int postIndex = lastValidIndex;


			// if(postLine == CenterlineScript.)


			float CoDriverCheckAheadDistanceSqr = CoDriverUIScript.CheckAheadDistanceStatic;

			if (CoDriverCheckAheadDistanceSqr > 0)
				CoDriverUIScript.UpdateArrowsStatic(linePoint.Item2, linePoint.Item1);

		}

		return true;
	}

	public void DebugPrint() {
		Debug.Log("reset");
	}
}
