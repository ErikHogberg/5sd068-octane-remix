using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class CenterlineProgressScript : MonoBehaviour {

	// Cheat mitigation
	int lastValidIndex = -1;
	CenterlineScript.InternalCenterline lastValidLine = null;

	// int furthestValidIndex = -1;
	// CenterlineScript.InternalCenterline furthestValidLine = null;

	bool waitForFinish = false;
	bool crossedFinish = false;

	private Vector3 resetPos = Vector3.zero;

	public float CheatMitigationSearchDistance = 10;
	public float CheatMitigationLookBehindDistance = 1;

	[Tooltip("largest valid distance from finishline in world space")]
	public float FinishLineCheckWorldRangeSqr = 1;
	[Tooltip("largest valid distance from finishline on the centerline")]
	public float FinishLineCheckCenterlineRangeSqr = 1;

	private CenterlineScript.InternalCenterline lastForkParent = null;
	public float CheatCheckPerSec = 5f;

	private float timer = -1;

	[Tooltip("If this component updates itself, querying progress every update")]
	public bool CheckInUpdate = false;
	[Tooltip("If laps aren't counted until the car touches a goal post trigger collider or goes too far from the finish line, as opposed to at the moment the centerline progress passes the finish line")]
	public bool WaitForGoalPost = true;

	public UnityEvent ResetEvent;
	public UnityEvent LapEvent;

	[HideInInspector]
	public bool MoveToStartOnNextQuery = false;

	// private void Start() {
	// 	LapEvent.AddListener(IncLap);
	// }

	void Update() {

		if (!CheckInUpdate)
			return;

		timer -= Time.deltaTime;
		if (timer < 0) {
			timer += 1f / CheatCheckPerSec;
			if (!QueryProgress(out bool lapCompleted)) {
				ResetEvent.Invoke();
				if (lapCompleted) {
					LapEvent.Invoke();
					IncLap();
				}
			}
		}

	}

	int laps = 0;
	void IncLap() {
		laps++;
	}

#if UNITY_EDITOR
	void OnDrawGizmos() {
		if (CenterlineScript.IsInitialized) {
			Gizmos.color = Color.magenta;

			Vector3 closestPos = CenterlineScript.GetClosestPointStatic(transform.position, out int index, out var fork, out float distance);

			// draw the line between the test object and the closest line point
			// FIXME: sometimes returns wrong closest
			Gizmos.DrawLine(transform.position, closestPos);

			if (lastValidLine != null && lastValidIndex >= 0 && lastValidIndex < lastValidLine.LinePoints.Count)
				Gizmos.DrawCube(CenterlineScript.MainInstanceTransform.TransformPoint(lastValidLine.LinePoints[lastValidIndex]), Vector3.one);

			Gizmos.color = Color.green;
			Gizmos.DrawLine(transform.position, resetPos);
			Gizmos.DrawCube(resetPos, Vector3.one);

			foreach ((int endIndex, var line, _) in CenterlineScript.GetRotationDeltasAhead(fork, CoDriverUIScript.CheckAheadDistanceStatic, index)) {
				Gizmos.color = Color.cyan;
				Gizmos.DrawCube(CenterlineScript.MainInstanceTransform.TransformPoint(line.LinePoints[endIndex]), Vector3.one);
			}

			if (lastValidLine != null)
				Handles.Label(transform.position, $"last valid: {lastValidLine.Name}, {lastValidIndex}");
			if (lastForkParent != null)
				Handles.Label(transform.position + Vector3.one * 5f, $"last fork parent: {lastForkParent.Name}");


			if (CheckInUpdate)
				Handles.Label(transform.position, $"lap: {laps}");

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

		// move car/object to start line of centerline if it has been flagged for a reset, iff also the centerline has a start line assigned
		// TODO: use centerline finish line as start line if start line is unassigned (but finish line isn't) 
		if (MoveToStartOnNextQuery) {
			MoveToStartOnNextQuery = false;
			var line = CenterlineScript.MainInstance.StartLine;
			int index = CenterlineScript.MainInstance.StartIndex;
			if (line != null && index >= 0 && index < line.LinePoints.Count) {
				resetPos = CenterlineScript.MainInstanceTransform.TransformPoint(line.LinePoints[index]);
				ResetTransform();
			}
		}

		CenterlineScript.InternalCenterline preLine = lastValidLine;
		int preIndex = lastValidIndex;

		if (lastValidLine == null || lastValidIndex < 0) {
			// start over progress with the closest point if the last valid point is unassigned
			resetPos = CenterlineScript.GetClosestPointStatic(transform.position, out int index, out var line, out float distance);
			lastValidIndex = index;
			lastValidLine = line;

			// still reset if the new start is too far away
			float resetDistance = lastValidLine.ResetDistance;//CenterlineScript.ResetDistanceStatic;
			if (resetDistance > 0 && distance > resetDistance) {
				// ResetTransform();
				return false;
			}
		} else {

			// FIXME: wont move onto forks which start at end of parent after lookbehind distance

			// check behind last valid point to see if there is a fork start in range
			(int checkStartIndex, float distanceBehindFoundSqr) = CenterlineScript.GetEarliestForkStartBehind(
				lastValidLine, CheatMitigationLookBehindDistance, lastValidIndex
			);

			float resetDistance = lastValidLine.ResetDistance;//CenterlineScript.ResetDistanceStatic;
			float distance;
			(int, CenterlineScript.InternalCenterline) linePoint;

			if (checkStartIndex < 0
			&& lastForkParent != null
			// && lastForkParent != lastValidLine
			) {
				// found fork start in range behind, and reference to last fork parent is valid

				// measure from fork start
				resetPos = CenterlineScript.GetClosestPointWithinRangeToIndexStatic(
					transform.position,
					lastForkParent,
					CheatMitigationSearchDistance * CheatMitigationSearchDistance + distanceBehindFoundSqr + CheatMitigationLookBehindDistance * CheatMitigationLookBehindDistance,
					out distance,
					out linePoint,
					lastValidLine.StartIndex
				);
			} else {

				// measure from last valid point
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


			if (lastValidLine != linePoint.Item2) {
				if (
					// lastValidLine.RejoinLine == linePoint.Item2 || 
					lastValidLine.Forks.Contains(linePoint.Item2)
					)
					lastForkParent = lastValidLine;
				else
					lastForkParent = null;
			}

			lastValidIndex = linePoint.Item1;
			lastValidLine = linePoint.Item2;

			if (resetDistance > 0 && distance > resetDistance) {
				// ResetTransform();
				return false;
			}


			CenterlineScript.InternalCenterline postLine = lastValidLine;
			int postIndex = lastValidIndex;

			if (waitForFinish) {
				if (!CenterlineScript.FinishLineInRangeStatic(lastValidLine.LinePoints[lastValidIndex], FinishLineCheckWorldRangeSqr)) {
					waitForFinish = false;
					crossedFinish = false;
				}
			} else if (CenterlineScript.CheckLapCrossStatic(preLine, preIndex, postLine, postIndex)) {
				if (WaitForGoalPost) {
					waitForFinish = true;
					if (crossedFinish) {
						lapCompleted = true;
					}
				} else {
					lapCompleted = true;
				}
			}


			float CoDriverCheckAheadDistanceSqr = CoDriverUIScript.CheckAheadDistanceStatic;

			if (CoDriverCheckAheadDistanceSqr > 0)
				CoDriverUIScript.UpdateArrowsStatic(linePoint.Item2, linePoint.Item1);

		}

		return true;
	}

	public bool ValidateFinishCrossing(out bool shouldReset) {

		if (!WaitForGoalPost) {
			shouldReset = false;
			return false;
		}

		shouldReset = true;


		// check if at all close to the finish line
		// if (!CenterlineScript.FinishLineInRangeStatic(transform.position, FinishLineCheckWorldRangeSqr)) {
		// 	return false;
		// }

		// check that last valid pos is in range of finish line
		if (!CenterlineScript.FinishLineInRangeStatic(lastValidLine, lastValidIndex, FinishLineCheckCenterlineRangeSqr)) {
			return false;
		}

		crossedFinish = true;

		// if not in range ahead, check if finish line was recently passed
		if (!waitForFinish) {
			shouldReset = false;
			return false;
		}

		// if (crossedFinish) {
		// 	shouldReset = false;
		// 	return false;
		// }

		shouldReset = false;
		return true;
	}

	public void SetLastValid(CenterlineScript.InternalCenterline line, int index) {
		lastValidLine = line;
		lastValidIndex = index;
	}

	public void DebugPrint() {
		Debug.Log("reset");
	}

}
