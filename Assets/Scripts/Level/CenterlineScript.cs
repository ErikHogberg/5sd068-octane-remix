using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CenterlineScript : MonoBehaviour, ISerializationCallbackReceiver {

	public const int MAX_DEPTH = 10;

	public class InternalCenterline {

		// TODO: active toggle for hiding branches from calculations at runtime due branching path circumventing the goal post
		// NOTE: dont serialize active state, because it is only used at runtime
		// TODO: tree-wide method for calculating which forks circumvent the finish line

		public string Name = "";
		public int StartIndex = 0;
		public List<Vector3> ControlPoints = new List<Vector3>();
		// IDEA: spatially partition line points for optimizing access and comparison operations, such as getting closest point on polyline
		public List<Vector3> LinePoints = new List<Vector3>();
		public int Resolution = 10;
		// IDEA: define rejoin index for defining where the line will end, on what index on a line (along with fork index for said line). used for looping lines or rejoining forks

		public bool ForksInspectorFoldState = false;
		public List<InternalCenterline> Forks = new List<InternalCenterline>();
		public InternalCenterline RejoinLine = null;
		public int RejoinIndex = 0;

	}

	[Serializable]
	public class SerializableInternalCenterline {

		public string Name = "";
		public int StartIndex = 0;
		public List<Vector3> ControlPoints = new List<Vector3>();
		public List<Vector3> LinePoints = new List<Vector3>();
		public int Resolution = 10;

		public bool ForksInspectorFoldState = false;
		public int childCount;
		public int indexOfFirstChild;

		public int indexOfRejoinLine;
		public int RejoinIndex;

	}

	public InternalCenterline MainCenterline = new InternalCenterline();
	public List<SerializableInternalCenterline> SerializedLines;

	// TODO: generate co-driver calls based on angle delta of set distance ahead of closest point
	// TODO: use centerline to pull car towards center of road as a handicap option
	// TODO: use centerline as respawn when falling off track
	//UINotificationSystem.Notify("Illegal shortcut!", Color.yellow, 1.5f);
	//ResetToCurrentSegment();
	// TODO: wire up goal posts to use centerline instead of road segments for catching skipping of goal posts and reversing into goal post

	// TODO: use centerline as cheat mitigation

	// TODO: method for getting closest point within defined index range ahead, or max distance ahead along curve
	// TODO: figure out way to make cheat mitigation (using index counting) work with forks/multiple lines

	public void OnBeforeSerialize() {
		if (SerializedLines == null) SerializedLines = new List<SerializableInternalCenterline>();
		if (MainCenterline == null) MainCenterline = new InternalCenterline();
		SerializedLines.Clear();
		AddNodeToSerializedNodes(MainCenterline);
	}

	void AddNodeToSerializedNodes(InternalCenterline n,
		List<(InternalCenterline, SerializableInternalCenterline)> rejoinRefResolveQueue = null,
		Dictionary<InternalCenterline, int> refCache = null
	) {

		if (rejoinRefResolveQueue == null) rejoinRefResolveQueue = new List<(InternalCenterline, SerializableInternalCenterline)>();
		if (refCache == null) refCache = new Dictionary<InternalCenterline, int>();

		var serializedNode = new SerializableInternalCenterline() {
			Name = n.Name,
			StartIndex = n.StartIndex,
			ControlPoints = n.ControlPoints,
			LinePoints = n.LinePoints,
			Resolution = n.Resolution,
			ForksInspectorFoldState = n.ForksInspectorFoldState,
			childCount = n.Forks.Count,
			indexOfFirstChild = SerializedLines.Count + 1,
			RejoinIndex = n.RejoinIndex,
		};

		for (int i = rejoinRefResolveQueue.Count - 1; i >= 0; i--) {
			if (rejoinRefResolveQueue[i].Item1 != n)
				continue;

			rejoinRefResolveQueue[i].Item2.indexOfRejoinLine = SerializedLines.Count;
			rejoinRefResolveQueue.RemoveAt(i);
		}

		if (n.RejoinLine != null) {
			if (refCache.TryGetValue(n.RejoinLine, out int rejoinLineIndex)) {
				serializedNode.indexOfRejoinLine = rejoinLineIndex;
			} else {
				rejoinRefResolveQueue.Add((n.RejoinLine, serializedNode));
			}
		}

		refCache.Add(n, SerializedLines.Count);

		SerializedLines.Add(serializedNode);
		foreach (var child in n.Forks)
			AddNodeToSerializedNodes(child, rejoinRefResolveQueue);
	}

	public void OnAfterDeserialize() {
		if (SerializedLines.Count > 0) {
			ReadNodeFromSerializedNodes(out MainCenterline);
		} else {
			MainCenterline = new InternalCenterline();
		}
	}

	int ReadNodeFromSerializedNodes(out InternalCenterline node, int index = 0,
		List<(int, InternalCenterline)> rejoinRefResolveQueue = null,
		Dictionary<int, InternalCenterline> refCache = null
	) {

		if (rejoinRefResolveQueue == null) rejoinRefResolveQueue = new List<(int, InternalCenterline)>();
		if (refCache == null) refCache = new Dictionary<int, InternalCenterline>();

		var serializedLine = SerializedLines[index];
		InternalCenterline newLine = new InternalCenterline() {
			Name = serializedLine.Name,
			StartIndex = serializedLine.StartIndex,
			ControlPoints = serializedLine.ControlPoints,
			LinePoints = serializedLine.LinePoints,
			Resolution = serializedLine.Resolution,
			ForksInspectorFoldState = serializedLine.ForksInspectorFoldState,
			Forks = new List<InternalCenterline>(),
			RejoinIndex = serializedLine.RejoinIndex
		};

		for (int i = rejoinRefResolveQueue.Count - 1; i >= 0; i--) {
			if (rejoinRefResolveQueue[i].Item1 != serializedLine.indexOfRejoinLine)
				continue;

			rejoinRefResolveQueue[i].Item2.RejoinLine = newLine;
			rejoinRefResolveQueue.RemoveAt(i);
		}

		refCache.Add(index, newLine);

		if (refCache.TryGetValue(serializedLine.indexOfRejoinLine, out InternalCenterline rejoinLine)) {
			newLine.RejoinLine = rejoinLine;
		} else {
			rejoinRefResolveQueue.Add((serializedLine.indexOfRejoinLine, newLine));
		}

		for (int i = 0; i != serializedLine.childCount; i++) {
			InternalCenterline childNode;
			index = ReadNodeFromSerializedNodes(out childNode, index + 1, rejoinRefResolveQueue, refCache);
			newLine.Forks.Add(childNode);
		}

		node = newLine;
		return index;
	}

#if UNITY_EDITOR
	void OnDrawGizmos() {

		UnityEditor.Handles.color = Color.white;

		DrawLine(MainCenterline);

	}

	void DrawLine(InternalCenterline line, int depth = 0) {
		if (depth > MAX_DEPTH) {
			Debug.LogError("Gizmo line draw recursion too deep");
			return;
		}

		if (line.LinePoints.Count < 1)
			return;

		for (int i = 1; i < line.LinePoints.Count; i++) {
			UnityEditor.Handles.DrawLine(
				transform.TransformPoint(line.LinePoints[i - 1]),
				transform.TransformPoint(line.LinePoints[i])
			);
		}

		foreach (var fork in line.Forks) {
			DrawLine(fork, depth + 1);
		}
	}
#endif

	public static List<Vector3> GenerateLinePoints(List<Vector3> ControlPoints, int Resolution) {
		List<Vector3> LinePoints = new List<Vector3>();

		int controlPointCount = ControlPoints.Count;

		int i = 0;
		while (i < controlPointCount) {
			int diff = controlPointCount - i;

			if (diff < 3) {
				if (i == 0) {
					if (controlPointCount == 1) {
						LinePoints.Add(ControlPoints[i]);
					}

					if (controlPointCount == 2) {
						LinePoints.Add(ControlPoints[i]);
						LinePoints.Add(ControlPoints[i + 1]);
					}

				} else {
					if (diff == 2) {
						Vector3 delta1 = ControlPoints[i] - ControlPoints[i - 1];
						Vector3 controlPoint1 = ControlPoints[i];
						Vector3 controlPoint2 = ControlPoints[i]
						 + delta1;
						Vector3 controlPoint3 = controlPoint2;
						Vector3 controlPoint4 = ControlPoints[i + 1];

						LinePoints.AddRange(Bezier.CubicBezierRender(
							controlPoint1,
							controlPoint2,
							controlPoint3,
							controlPoint4,
							Resolution
						));
					}
				}

				break;
			}

			if (i == 0) {
				LinePoints.AddRange(Bezier.CubicBezierRender(
					ControlPoints[0],
					ControlPoints[1],
					ControlPoints[1],
					ControlPoints[2],
					Resolution
				));
				i += 2;

				continue;
			}


			{
				// TODO: generate first point from direction of last 2 line points
				// IDEA: cache last 2 line points generated, or angle between them
				// IDEA: use previous control points delta instead
				Vector3 delta1 = ControlPoints[i] - ControlPoints[i - 1];
				Vector3 controlPoint1 = ControlPoints[i];
				Vector3 controlPoint2 = ControlPoints[i]
				 + delta1;
				Vector3 controlPoint3 = ControlPoints[i + 1];
				Vector3 controlPoint4 = ControlPoints[i + 2];

				LinePoints.AddRange(Bezier.CubicBezierRender(
					controlPoint1,
					controlPoint2,
					controlPoint3,
					controlPoint4,
					Resolution
				));

				i += 2;
			}

		}

		return LinePoints;
	}

	public void GenerateLinePoints() {
		GenerateLinePoints(MainCenterline);
	}

	static void GenerateLinePoints(InternalCenterline line, Vector3? lineStartPoint = null, int depth = 0) {

		if (depth > MAX_DEPTH) {
			Debug.LogError("Generate line points recursion too deep");
			return;
		}

		List<Vector3> controlPoints = line.ControlPoints;
		if (lineStartPoint is Vector3 startPoint) {
			// controlPoints = new List<Vector3>{startPoint}.AddRange(controlPoints);
			if (line.RejoinLine != null) {
				int rejoinIndex = line.RejoinIndex >= line.RejoinLine.LinePoints.Count ? line.RejoinLine.LinePoints.Count - 1 : line.RejoinIndex;
				controlPoints = controlPoints.Prepend(startPoint).Append(line.RejoinLine.LinePoints[rejoinIndex]).ToList();
			} else {
				// TODO: check performance impact, find better alternative of altering list
				controlPoints = controlPoints.Prepend(startPoint).ToList();
			}
		} else if (line.RejoinLine != null) {
			int rejoinIndex = line.RejoinIndex >= line.RejoinLine.LinePoints.Count ? line.RejoinLine.LinePoints.Count - 1 : line.RejoinIndex;
			controlPoints = controlPoints.Append(line.RejoinLine.LinePoints[rejoinIndex]).ToList();
		}

		line.LinePoints = GenerateLinePoints(controlPoints, line.Resolution);

		foreach (var fork in line.Forks) {
			Vector3 forkStartPoint = line.LinePoints != null && line.LinePoints.Count > fork.StartIndex && fork.StartIndex >= 0 ? line.LinePoints[fork.StartIndex] : Vector3.zero;
			GenerateLinePoints(fork, forkStartPoint, depth + 1);
		}

	}

	/// Gets all rotation deltas of paths ahead (current line + any new forks in the distance ahead)
	/// Measured between the direction of closest point on line towards the next point after it, and the direction of the given other point on the line towards the previous point behind it.
	/// (Index at end, line at end, rotation delta)
	public IEnumerable<(int, InternalCenterline, Quaternion)> GetRotationDeltaAhead(Vector3 pos, float distanceAhead) {
		Vector3 closestPos = GetClosestPoint(pos, out int index, out InternalCenterline line, out float distance);
		// return GetRotationDeltaAhead(closestPos, index, distanceAhead, forkIndex);
		return GetRotationDeltasAhead(line, distanceAhead * distanceAhead, index);
	}

	public static IEnumerable<(int, InternalCenterline, Quaternion)> GetRotationDeltasAhead(
		InternalCenterline line,
		float distanceAheadSqr,
		int startIndex = 0,
		// Quaternion? compareRot = null, 
		int depth = 0
	) {
		// IDEA: search backwards if distance is negative

		if (depth > MAX_DEPTH) {
			Debug.LogError("Get rotation delta recursion too deep");
			yield break;
		}

		// Quaternion compareRotValue = compareRot ?? Quaternion.LookRotation(line.LinePoints[startIndex + 1] - line.LinePoints[startIndex], Vector3.up);

		float distanceTraveledSqr = 0;
		for (int i = startIndex + 1; i < line.LinePoints.Count; i++) {
			// calculate distance from previous point
			float distanceSqr = (line.LinePoints[i] - line.LinePoints[i - 1]).sqrMagnitude; // NOTE: does not use transform scale, distance ahead is relative to internal point measurement

			// accumulate distance traveled so far
			distanceTraveledSqr += distanceSqr;

			if (distanceTraveledSqr < distanceAheadSqr) {
				// keep on going if accumulated distance has not yet passed distance ahead to measure
				// NOTE: wont return any value if the distance ahead to measure overshoots the remaining length of the line
				continue;
			} else {
				int compareLineIndex = i;

				// get psuedo-tangent of the curve at this point (by measuring direction from the previous point)
				Quaternion outRot = Quaternion.LookRotation(line.LinePoints[i] - line.LinePoints[i - 1], Vector3.up);

				yield return (compareLineIndex, line, outRot);
				break;
			}
		}

		if (distanceTraveledSqr < distanceAheadSqr && line.RejoinLine != null) {
			foreach (var rejoinResult in GetRotationDeltasAhead(line.RejoinLine, distanceAheadSqr - distanceTraveledSqr, line.RejoinIndex, depth + 1))
				yield return rejoinResult;
		}

		foreach (var fork in line.Forks) {
			if (fork.StartIndex < startIndex)
				continue;

			float forkDistanceAheadSqr = distanceAheadSqr;
			for (int i = startIndex + 1; i <= fork.StartIndex; i++) {
				float distanceSqr = (line.LinePoints[i] - line.LinePoints[i - 1]).sqrMagnitude; // NOTE: does not use transform scale, distance ahead is relative to internal point measurement
				forkDistanceAheadSqr -= distanceSqr;
			}

			if (forkDistanceAheadSqr < 0)
				continue;

			foreach (var forkResult in GetRotationDeltasAhead(
				fork,
				forkDistanceAheadSqr,
				0,
				// compareRotValue, 
				depth + 1
				)) {
				yield return forkResult;
			}
		}

	}

	/// Gets all of the *greatest* rotation deltas of the paths ahead (current line + any new forks in the distance ahead). 
	/// Measured between the direction of closest point on line towards the next point after it, and the direction of the given other point on the line towards the previous point behind it.
	/// (Index at end, index at greates delta, fork index, rotation delta)
	public static IEnumerable<(int, int, InternalCenterline, Quaternion)> GetGreatestRotationDeltasAhead(
		InternalCenterline line,
		float distanceAheadSqr,
		int startIndex = 0,
		Quaternion? compareRot = null,
		int depth = 0
	) {

		// IDEA: search backwards if distance is negative?
		// IDEA: option to ignore some distance at the start in front of the car

		// IDEA: option to only return once with greatest delta of all forks

		if (depth > MAX_DEPTH) {
			Debug.LogError("Get greatest rotation delta recursion too deep");
			yield break;
		}

		float distanceTraveledSqr = 0;
		Quaternion greatestDelta = Quaternion.identity;
		float greatestDeltaAngle = 0;
		int indexAtGreatestDelta = -1;

		Quaternion compareRotValue = compareRot ?? Quaternion.LookRotation(line.LinePoints[startIndex + 1] - line.LinePoints[startIndex], Vector3.up);

		for (int i = startIndex + 1; i < line.LinePoints.Count; i++) {
			float distanceSqr = (line.LinePoints[i] - line.LinePoints[i - 1]).sqrMagnitude;
			
			distanceTraveledSqr += distanceSqr;
			Quaternion outRot =
				Quaternion.LookRotation(line.LinePoints[i] - line.LinePoints[i - 1], Vector3.up)
				// * inverseRot
				;

			float angle = Quaternion.Angle(
				// Quaternion.identity, 
				compareRotValue,
				outRot
				);
			if (greatestDeltaAngle <= angle) {
				greatestDeltaAngle = angle;
				greatestDelta = outRot;
				indexAtGreatestDelta = i;
			}

			if (distanceTraveledSqr < distanceAheadSqr) {
				continue;
			} else {
				yield return (i, indexAtGreatestDelta, line, greatestDelta);
				break;
			}
		}

		if (distanceTraveledSqr < distanceAheadSqr && line.RejoinLine != null) {
			foreach (var rejoinResult in GetGreatestRotationDeltasAhead(line.RejoinLine, distanceAheadSqr - distanceTraveledSqr, line.RejoinIndex, compareRot, depth + 1))
				yield return rejoinResult;
		}

		foreach (var fork in line.Forks) {
			if (fork.StartIndex < startIndex)
				continue;

			float forkDistanceAheadSqr = distanceAheadSqr;
			for (int i = startIndex + 1; i <= fork.StartIndex; i++) {
				float distanceSqr = (line.LinePoints[i] - line.LinePoints[i - 1]).sqrMagnitude; // NOTE: does not use transform scale, distance ahead is relative to internal point measurement
				forkDistanceAheadSqr -= distanceSqr;
			}

			if (forkDistanceAheadSqr < 0)
				continue;

			foreach (var forkResult in GetGreatestRotationDeltasAhead(fork, forkDistanceAheadSqr, 0, compareRotValue, depth + 1))
				yield return forkResult;
		}

	}

	public Vector3 GetClosestPoint(Vector3 pos, out int closestLineIndex, out InternalCenterline closestLine, out float closestDistance) {
		return GetClosestPoint(pos, MainCenterline, transform, out closestLineIndex, out closestLine, out closestDistance);
	}

	public static Vector3 GetClosestPoint(Vector3 pos, InternalCenterline line, Transform transform,
		out int closestLineIndex,
		out InternalCenterline closestLine,
		out float closestDistance,
		int depth = 0
	) {

		pos = transform.InverseTransformPoint(pos);

		Vector3 currentClosest = Vector3.zero;
		float currentClosestDistance = 0;
		int lineIndex = 0;
		// closestForkIndex = -1;
		closestLine = line;



		for (int i = 1; i < line.LinePoints.Count; i++) {
			float distance = distanceToSegment(line.LinePoints[i - 1], line.LinePoints[i], pos);

			if (i == 1) {
				currentClosestDistance = distance;
				currentClosest = ProjectPointOnLineSegment(line.LinePoints[i - 1], line.LinePoints[i], pos);
				lineIndex = 0;
				continue;
			}

			if (distance < currentClosestDistance) {
				currentClosestDistance = distance;
				currentClosest = ProjectPointOnLineSegment(line.LinePoints[i - 1], line.LinePoints[i], pos);
				lineIndex = i - 1;
			}
		}

		if (depth > MAX_DEPTH) {
			Debug.LogError("Get closest point recursion too deep");
			closestDistance = currentClosestDistance;
			closestLineIndex = lineIndex;
			return currentClosest;
		}

		foreach (var fork in line.Forks) {
			int forkClosestIndex;
			InternalCenterline forkClosestLine;
			float forkClosestDistance;
			Vector3 forkClosestPos = GetClosestPoint(pos, fork, transform, out forkClosestIndex, out forkClosestLine, out forkClosestDistance);
			if (forkClosestDistance < currentClosestDistance) {
				currentClosestDistance = forkClosestDistance;
				currentClosest = forkClosestPos;
				closestLine = forkClosestLine;
				lineIndex = forkClosestIndex;
			}
		}

		closestDistance = currentClosestDistance;
		closestLineIndex = lineIndex;
		return currentClosest;
	}

	public Vector3 GetClosestPoint(Vector3 pos) {
		return GetClosestPoint(pos, out int _index, out InternalCenterline _line, out float _closestDistance);
	}

	public Vector3 GetClosestPoint(Vector3 pos, out int closestLineIndex, out InternalCenterline closestFork) {
		Vector3 outPos = GetClosestPoint(pos, out int index, out InternalCenterline line, out float _closestDistance);
		closestLineIndex = index;
		closestFork = line;
		return outPos;
	}

	public Vector3 GetClosestPoint(Vector3 pos, out float closestDistance) {
		Vector3 outPos = GetClosestPoint(pos, out int _index, out InternalCenterline _line, out float distance);
		closestDistance = distance;
		return outPos;
	}

	public Vector3 GetClosestPointWithinRangeToIndex(Vector3 pos, int relativeIndex, int relativeForkIndex, float maxCurveDistanceAhead, out int closestLineIndex, out float closestDistance) {
		Vector3 currentClosest = Vector3.zero;
		float currentClosestDistance = 0;
		int lineIndex = 0;

		// float distanceAccumulatorSqr = 0;



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

		// TODO: check forks too
		// TODO: handle starting/relative index on fork
		// for (int forkIndex = 0; forkIndex < Forks.Count; forkIndex++) {
		// 	for (int i = relativeIndex; i < MainCenterline.LinePoints.Count; i++) {
		// 		float distance = distanceToSegment(Forks[forkIndex].LinePoints[i - 1], Forks[forkIndex].LinePoints[i], pos);
		// 		// TODO: add distance between line points to accumulator, stop if accumulator passes max curve distance

		// 		if (distance < currentClosestDistance) {
		// 			currentClosestDistance = distance;
		// 			currentClosest = ProjectPointOnLineSegment(Forks[forkIndex].LinePoints[i - 1], Forks[forkIndex].LinePoints[i], pos);
		// 			lineIndex = i - 1;
		// 		}
		// 	}
		// }

		closestDistance = currentClosestDistance;
		closestLineIndex = lineIndex;
		return currentClosest;
	}

	public Vector2 GetUIArrowDir(Quaternion rot) {
		Vector3 direction = rot * Vector3.forward;
		Vector3 projection = Vector3.ProjectOnPlane(direction, Vector3.back);

		return new Vector2(projection.x, projection.y);
	}

	public Vector2 GetUIArrowDir(Quaternion fromRot, Quaternion toRot) {
		Quaternion rot = toRot * Quaternion.Inverse(fromRot);
		return GetUIArrowDir(rot);
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
