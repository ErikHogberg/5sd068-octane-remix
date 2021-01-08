using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterlineScript : MonoBehaviour {

	public List<Vector3> ControlPoints;

	// IDEA: spatially partition line points for optimizing access and comparison operations, such as getting closest point on polyline
	public List<Vector3> LinePoints;

	public int Resolution = 10;

	// TODO: generate co-driver calls based on angle delta of set distance ahead of closest point

#if UNITY_EDITOR
	void OnDrawGizmos() {
		UnityEditor.Handles.color = Color.white;

		for (int i = 1; i < LinePoints.Count; i++) {
			UnityEditor.Handles.DrawLine(
				transform.TransformPoint(LinePoints[i - 1]),
				transform.TransformPoint(LinePoints[i])
			);

		}
	}
#endif

	public void GenerateLinePoints() {
		int controlPointCount = ControlPoints.Count;
		if (controlPointCount < 2)
			return;

		if (LinePoints == null) {
			LinePoints = new List<Vector3>();
		} else {
			LinePoints.Clear();
		}

		if (controlPointCount == 2) {
			LinePoints.Add(ControlPoints[0]);
			LinePoints.Add(ControlPoints[1]);
			return;
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

	public Quaternion GetRotationDeltaAhead(Vector3 pos, float distanceAhead, out int compareLineIndex) {
		Vector3 closestPos = GetClosestPoint(pos, out int index, out float distance);
		Quaternion outRot = GetRotationDeltaAhead(closestPos, index, distanceAhead, out int outCompareLineIndex);
		compareLineIndex = outCompareLineIndex;
		return outRot;
	}

	public Quaternion GetRotationDeltaAhead(Vector3 closestPos, int closestLineIndex, float distanceAhead, out int compareLineIndex) {
		// TODO: search backwards if distance is negative

		int index = closestLineIndex;
		float distanceAheadSqr = distanceAhead * distanceAhead;
		float distanceTraveledSqr = 0;
		for (int i = index + 1; i < LinePoints.Count; i++) {
			float distanceSqr = (LinePoints[i] - closestPos).sqrMagnitude;
			distanceTraveledSqr += distanceSqr;
			if (distanceTraveledSqr < distanceAheadSqr) {
				continue;
			} else {
				compareLineIndex = i;

				Quaternion outRot = 
				Quaternion.LookRotation(LinePoints[i] - LinePoints[i - 1],Vector3.up) 
				
				* 
				Quaternion.Inverse(
					Quaternion.LookRotation(LinePoints[index + 1] - LinePoints[index],Vector3.up)
					);
					
				return outRot;
			}
		}

		compareLineIndex = -1;
		return Quaternion.identity;
	}

	public Vector3 GetClosestPoint(Vector3 pos, out int closestLineIndex, out float closestDistance) {
		Vector3 currentClosest = Vector3.zero;
		float currentClosestDistance = 0;
		int lineIndex = 0;


		for (int i = 1; i < LinePoints.Count; i++) {
			float distance = distanceToSegment(LinePoints[i - 1], LinePoints[i], pos);

			if (i == 1) {
				currentClosestDistance = distance;
				currentClosest = ProjectPointOnLineSegment(LinePoints[i - 1], LinePoints[i], pos);
				lineIndex = 0;
				continue;
			}

			if (distance < currentClosestDistance) {
				currentClosestDistance = distance;
				currentClosest = ProjectPointOnLineSegment(LinePoints[i - 1], LinePoints[i], pos);
				lineIndex = i - 1;
			}
		}

		closestDistance = currentClosestDistance;
		closestLineIndex = lineIndex;
		return currentClosest;
	}

	public Vector3 GetClosestPoint(Vector3 pos) {
		return GetClosestPoint(pos, out int _index, out float _closestDistance);
	}

	public Vector3 GetClosestPoint(Vector3 pos, out int closestLineIndex) {
		Vector3 outPos = GetClosestPoint(pos, out int index, out float _closestDistance);
		closestLineIndex = index;
		return outPos;
	}

	public Vector3 GetClosestPoint(Vector3 pos, out float closestDistance) {
		Vector3 outPos = GetClosestPoint(pos, out int _index, out float distance);
		closestDistance = distance;
		return outPos;
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
