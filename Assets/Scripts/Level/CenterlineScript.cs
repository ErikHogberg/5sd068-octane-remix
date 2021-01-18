using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CenterlineScript : MonoBehaviour {

	// public static List<CenterlineScript> Instances = new List<CenterlineScript>();

	[Serializable]
	public class InternalCenterline {
		public int StartIndex = 0;
		public List<Vector3> ControlPoints = new List<Vector3>();
		// IDEA: spatially partition line points for optimizing access and comparison operations, such as getting closest point on polyline
		public List<Vector3> LinePoints = new List<Vector3>();
		public int Resolution = 10;
		// IDEA: define rejoin index for defining where the line will end, on what index on a line (along with fork index for said line). used for looping lines or rejoining forks
	}

	public InternalCenterline MainCenterline = new InternalCenterline();
	public List<InternalCenterline> Forks = new List<InternalCenterline>();
	// IDEA: make forks forkable?

	// TODO: generate co-driver calls based on angle delta of set distance ahead of closest point
	// TODO: use centerline to pull car towards center of road as a handicap option
	// TODO: use centerline as respawn when falling off track
	// TODO: use centerline as cheat mitigation

	// TODO: method for getting closest point within defined index range ahead, or max distance ahead along curve
	// TODO: figure out way to make cheat mitigation (using index counting) work with forks/multiple lines

	// private void Awake() {
	// 	if(!Instances.Contains(this)){
	// 		Instances.Add(this);
	// 	}
	// }

	// private void OnDestroy() {
	// 	Instances.Remove(this);
	// }

#if UNITY_EDITOR
	void OnDrawGizmos() {
		UnityEditor.Handles.color = Color.white;

		for (int i = 1; i < MainCenterline.LinePoints.Count; i++) {
			UnityEditor.Handles.DrawLine(
				transform.TransformPoint(MainCenterline.LinePoints[i - 1]),
				transform.TransformPoint(MainCenterline.LinePoints[i])
			);
		}

		foreach (var fork in Forks) {
			if (fork.LinePoints.Count < 1)
				continue;

			for (int i = 1; i < fork.LinePoints.Count; i++) {
				UnityEditor.Handles.DrawLine(
					transform.TransformPoint(fork.LinePoints[i - 1]),
					transform.TransformPoint(fork.LinePoints[i])
				);
			}
		}

	}
#endif

	public static List<Vector3> GenerateLinePoints(Vector3? startControlPoint, List<Vector3> ControlPoints, int Resolution) {
		List<Vector3> LinePoints = new List<Vector3>();

		int controlPointCount = ControlPoints.Count;

		if (startControlPoint is Vector3 start) {
			if (controlPointCount < 1)
				return LinePoints;

			if (controlPointCount == 1) {
				LinePoints.Add(start);
				LinePoints.Add(ControlPoints[0]);
				return LinePoints;
			}


			if (controlPointCount == 2) {
				LinePoints = Bezier.CubicBezierRender(
					start,
					ControlPoints[0],
					ControlPoints[0],
					ControlPoints[1],
					Resolution
				);
			}

			if (controlPointCount == 3) {
				LinePoints = Bezier.CubicBezierRender(
					start,
					ControlPoints[0],
					ControlPoints[1],
					ControlPoints[2],
					Resolution
				);
			}

			if (controlPointCount > 3) {
				LinePoints = Bezier.CubicBezierRender(
					ControlPoints.Prepend(start).ToList(),
					Resolution
				);
			}
		} else {
			if (controlPointCount < 2)
				return LinePoints;

			if (controlPointCount == 2) {
				LinePoints.Add(ControlPoints[0]);
				LinePoints.Add(ControlPoints[1]);
				return LinePoints;
			}

			if (controlPointCount == 3) {
				LinePoints = Bezier.CubicBezierRender(
					ControlPoints[0],
					ControlPoints[1],
					ControlPoints[1],
					ControlPoints[2],
					Resolution
				);
			}

			if (controlPointCount == 4) {
				LinePoints = Bezier.CubicBezierRender(
					ControlPoints[0],
					ControlPoints[1],
					ControlPoints[2],
					ControlPoints[3],
					Resolution
				);
			}

			if (controlPointCount > 4) {
				LinePoints = Bezier.CubicBezierRender(
					ControlPoints,
					Resolution
				);
			}
		}

		return LinePoints;
	}

	public void GenerateLinePoints() {

		MainCenterline.LinePoints = GenerateLinePoints(null, MainCenterline.ControlPoints, MainCenterline.Resolution);

		foreach (var fork in Forks) {
			Vector3 startPoint = MainCenterline.LinePoints != null && MainCenterline.LinePoints.Count > fork.StartIndex && fork.StartIndex >= 0 ? MainCenterline.LinePoints[fork.StartIndex] : Vector3.zero;
			fork.LinePoints = GenerateLinePoints(startPoint, fork.ControlPoints, fork.Resolution);
		}

	}

	public IEnumerable<(int, int, Quaternion)> GetRotationDeltaAhead(Vector3 pos, float distanceAhead) {
		Vector3 closestPos = GetClosestPoint(pos, out int index, out int forkIndex, out float distance);
		var outRot = GetRotationDeltaAhead(closestPos, index, distanceAhead, forkIndex);
		return outRot;
	}

	public IEnumerable<(int, int, Quaternion)> GetRotationDeltaAhead(Vector3 closestPos, int closestLineIndex, float distanceAhead, int startFork = -1) {
		// TODO: search backwards if distance is negative
		// TODO: handle forks

		float distanceAheadSqr = distanceAhead * distanceAhead;
		int index = closestLineIndex;
		Quaternion inverseRot = Quaternion.Inverse(Quaternion.LookRotation(MainCenterline.LinePoints[index + 1] - MainCenterline.LinePoints[index], Vector3.up));

		if (startFork < 0) {

			float distanceTraveledSqr = 0;
			for (int i = index + 1; i < MainCenterline.LinePoints.Count; i++) {
				float distanceSqr = (MainCenterline.LinePoints[i] - closestPos).sqrMagnitude;
				distanceTraveledSqr += distanceSqr;
				if (distanceTraveledSqr < distanceAheadSqr) {
					continue;
				} else {
					int compareLineIndex = i;

					Quaternion outRot = Quaternion.LookRotation(MainCenterline.LinePoints[i] - MainCenterline.LinePoints[i - 1], Vector3.up)
						* inverseRot;

					yield return (compareLineIndex, -1, outRot);
					break;
				}
			}

			distanceTraveledSqr = 0;
			for (int forkIndex = 0; forkIndex < Forks.Count; forkIndex++) {
				if (Forks[forkIndex].StartIndex < index) {
					continue;
				}
				// TODO: add to accumulator distance from start on centerline until start of fork
				for (int i = index + 1; i < Forks[forkIndex].StartIndex; i++) {
					float distanceSqr = (MainCenterline.LinePoints[i] - closestPos).sqrMagnitude;
					distanceTraveledSqr += distanceSqr;
				}

				for (int i = index + 1; i < Forks[forkIndex].LinePoints.Count; i++) {
					float distanceSqr = (MainCenterline.LinePoints[i] - closestPos).sqrMagnitude;
					distanceTraveledSqr += distanceSqr;
					if (distanceTraveledSqr < distanceAheadSqr) {
						continue;
					} else {
						int compareLineIndex = i;

						Quaternion outRot = Quaternion.LookRotation(Forks[forkIndex].LinePoints[i] - Forks[forkIndex].LinePoints[i - 1], Vector3.up)
							* inverseRot;

						yield return (compareLineIndex, forkIndex, outRot);
						break;
					}
				}
			}
		} else {
			float distanceTraveledSqr = 0;
			for (int i = index + 1; i < Forks[startFork].LinePoints.Count; i++) {
				float distanceSqr = (MainCenterline.LinePoints[i] - closestPos).sqrMagnitude;
				distanceTraveledSqr += distanceSqr;
				if (distanceTraveledSqr < distanceAheadSqr) {
					continue;
				} else {
					int compareLineIndex = i;

					Quaternion outRot = Quaternion.LookRotation(MainCenterline.LinePoints[i] - MainCenterline.LinePoints[i - 1], Vector3.up)
						* inverseRot;

					yield return (compareLineIndex, -1, outRot);
					break;
				}
			}
		}

		// compareLineIndex = -1;
		// yield return Quaternion.identity;
	}

	public IEnumerable<(int, int, Quaternion)> GetGreatestRotationDeltaAhead(Vector3 closestPos, int closestLineIndex, float distanceAhead) {
		// TODO: search backwards if distance is negative
		// TODO: option to ignore some distance at the start in front of the car
		// TODO: handle forks

		int index = closestLineIndex;
		float distanceAheadSqr = distanceAhead * distanceAhead;
		float distanceTraveledSqr = 0;
		Quaternion greatestDelta = Quaternion.identity;
		float greatestDeltaAngle = 0;
		int indexAtGreatestDelta = -1;

		int indexAtEnd = -1;

		for (int i = index + 1; i < MainCenterline.LinePoints.Count; i++) {
			float distanceSqr = (MainCenterline.LinePoints[i] - closestPos).sqrMagnitude;
			distanceTraveledSqr += distanceSqr;
			Quaternion outRot =
				Quaternion.LookRotation(MainCenterline.LinePoints[i] - MainCenterline.LinePoints[i - 1], Vector3.up)
				* Quaternion.Inverse(
					Quaternion.LookRotation(MainCenterline.LinePoints[index + 1] - MainCenterline.LinePoints[index], Vector3.up)
				);

			float angle = Quaternion.Angle(Quaternion.identity, outRot);
			if (greatestDeltaAngle < angle) {
				greatestDeltaAngle = angle;
				greatestDelta = outRot;
				indexAtGreatestDelta = i;
			}

			if (distanceTraveledSqr < distanceAheadSqr) {
				continue;
			} else {
				indexAtEnd = i;
				yield return (indexAtEnd, indexAtGreatestDelta, greatestDelta);
			}
		}

		// return Quaternion.identity;
	}

	public Vector3 GetClosestPoint(Vector3 pos, out int closestLineIndex, out int closestForkIndex, out float closestDistance) {

		pos = transform.InverseTransformPoint(pos);

		Vector3 currentClosest = Vector3.zero;
		float currentClosestDistance = 0;
		int lineIndex = 0;
		closestForkIndex = -1;


		for (int i = 1; i < MainCenterline.LinePoints.Count; i++) {
			float distance = distanceToSegment(MainCenterline.LinePoints[i - 1], MainCenterline.LinePoints[i], pos);

			if (i == 1) {
				currentClosestDistance = distance;
				currentClosest = ProjectPointOnLineSegment(MainCenterline.LinePoints[i - 1], MainCenterline.LinePoints[i], pos);
				lineIndex = 0;
				continue;
			}

			if (distance < currentClosestDistance) {
				currentClosestDistance = distance;
				currentClosest = ProjectPointOnLineSegment(MainCenterline.LinePoints[i - 1], MainCenterline.LinePoints[i], pos);
				lineIndex = i - 1;
			}
		}


		for (int forkIndex = 0; forkIndex < Forks.Count; forkIndex++) {
			for (int i = 1; i < Forks[forkIndex].LinePoints.Count; i++) {
				float distance = distanceToSegment(Forks[forkIndex].LinePoints[i - 1], Forks[forkIndex].LinePoints[i], pos);

				if (distance < currentClosestDistance) {
					currentClosestDistance = distance;
					currentClosest = ProjectPointOnLineSegment(Forks[forkIndex].LinePoints[i - 1], Forks[forkIndex].LinePoints[i], pos);
					lineIndex = i - 1;
					closestForkIndex = forkIndex;
				}
			}
		}

		closestDistance = currentClosestDistance;
		closestLineIndex = lineIndex;
		return currentClosest;
	}

	public Vector3 GetClosestPoint(Vector3 pos) {
		return GetClosestPoint(pos, out int _index, out int _forkIndex, out float _closestDistance);
	}

	public Vector3 GetClosestPoint(Vector3 pos, out int closestLineIndex, out int closestForkIndex) {
		Vector3 outPos = GetClosestPoint(pos, out int index, out int forkIndex, out float _closestDistance);
		closestLineIndex = index;
		closestForkIndex = forkIndex;
		return outPos;
	}

	public Vector3 GetClosestPoint(Vector3 pos, out float closestDistance) {
		Vector3 outPos = GetClosestPoint(pos, out int _index, out int _forkIndex, out float distance);
		closestDistance = distance;
		return outPos;
	}

	public Vector3 GetClosestPointWithinRangeToIndex(Vector3 pos, int relativeIndex, int relativeForkIndex, float maxCurveDistanceAhead, out int closestLineIndex, out float closestDistance) {
		Vector3 currentClosest = Vector3.zero;
		float currentClosestDistance = 0;
		int lineIndex = 0;

		// float distanceAccumulatorSqr = 0;

		// TODO: check forks too
		// TODO: handle starting/relative index on fork

		for (int i = relativeIndex; i < MainCenterline.LinePoints.Count; i++) {
			float distance = distanceToSegment(MainCenterline.LinePoints[i - 1], MainCenterline.LinePoints[i], pos);
			// TODO: add distance between line points to accumulator, stop if accumulator passes max curve distance

			if (i == 1) {
				currentClosestDistance = distance;
				currentClosest = ProjectPointOnLineSegment(MainCenterline.LinePoints[i - 1], MainCenterline.LinePoints[i], pos);
				lineIndex = 0;
				continue;
			}

			if (distance < currentClosestDistance) {
				currentClosestDistance = distance;
				currentClosest = ProjectPointOnLineSegment(MainCenterline.LinePoints[i - 1], MainCenterline.LinePoints[i], pos);
				lineIndex = i - 1;
			}
		}

		for (int forkIndex = 0; forkIndex < Forks.Count; forkIndex++) {
			for (int i = relativeIndex; i < MainCenterline.LinePoints.Count; i++) {
				float distance = distanceToSegment(Forks[forkIndex].LinePoints[i - 1], Forks[forkIndex].LinePoints[i], pos);
				// TODO: add distance between line points to accumulator, stop if accumulator passes max curve distance

				if (distance < currentClosestDistance) {
					currentClosestDistance = distance;
					currentClosest = ProjectPointOnLineSegment(Forks[forkIndex].LinePoints[i - 1], Forks[forkIndex].LinePoints[i], pos);
					lineIndex = i - 1;
				}
			}
		}

		closestDistance = currentClosestDistance;
		closestLineIndex = lineIndex;
		return currentClosest;
	}

	public static float distanceToSegment(Vector3 lineStart, Vector3 lineEnd, Vector3 pos) {
		var startToEnd = lineEnd - lineStart;

		var startToPos = pos - lineStart;
		if (Vector3.Dot(startToPos, startToEnd) <= 0f)
			return startToPos.magnitude;

		var endToPos = pos - lineEnd;
		if (Vector3.Dot(endToPos, startToEnd) >= 0f)
			return endToPos.magnitude;

		return Vector3.Cross(startToEnd, startToPos).magnitude / startToEnd.magnitude;
	}

	// src: http://wiki.unity3d.com/index.php/3d_Math_functions
	public static Vector3 ProjectPointOnLine(Vector3 linePoint, Vector3 lineVec, Vector3 point) {

		//get vector from point on line to point in space
		Vector3 linePointToPoint = point - linePoint;

		float t = Vector3.Dot(linePointToPoint, lineVec);

		return linePoint + lineVec * t;
	}

	// src: http://wiki.unity3d.com/index.php/3d_Math_functions
	public static int PointOnWhichSideOfLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point) {

		Vector3 lineVec = linePoint2 - linePoint1;
		Vector3 pointVec = point - linePoint1;

		float dot = Vector3.Dot(pointVec, lineVec);

		//point is on side of linePoint2, compared to linePoint1
		if (dot > 0) {

			//point is on the line segment
			if (pointVec.magnitude <= lineVec.magnitude) {

				return 0;
			}

			//point is not on the line segment and it is on the side of linePoint2
			else {

				return 2;
			}
		}

		//Point is not on side of linePoint2, compared to linePoint1.
		//Point is not on the line segment and it is on the side of linePoint1.
		else {

			return 1;
		}
	}

	// src: http://wiki.unity3d.com/index.php/3d_Math_functions
	public static Vector3 ProjectPointOnLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point) {

		Vector3 vector = linePoint2 - linePoint1;

		Vector3 projectedPoint = ProjectPointOnLine(linePoint1, vector.normalized, point);

		int side = PointOnWhichSideOfLineSegment(linePoint1, linePoint2, projectedPoint);

		//The projected point is on the line segment
		if (side == 0) {

			return projectedPoint;
		}

		if (side == 1) {

			return linePoint1;
		}

		if (side == 2) {

			return linePoint2;
		}

		//output is invalid
		return Vector3.zero;
	}

}
