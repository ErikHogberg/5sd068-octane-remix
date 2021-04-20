using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// Cyclical tree data structure containing bezier curves, representing the 3D paths a car is allowed to take through a track
/// Meant to be used as a replacement for the old track progression, cheat mitigation, moving goal post, and reset systems
/// Features this system is also planned to add is generating rally co-driver cars (warnings of curves and dangers ahead), 
/// a handicap system that pull the car towards the center of the road, 
public class CenterlineScript : MonoBehaviour, ISerializationCallbackReceiver {

	public const int MAX_DEPTH = 10;

	// non-serialized run-time data container
	public class InternalCenterline {

		// runtime-only, non-serialized active state which includes/excludes the line from queries
		// TODO: implement queries responding to active state
		public bool Active = true;
		// runtime-only, non-serialized line index which causes the part of the line past this index to be excluded from queries (disabled if index is below 0)
		// TODO: 
		// TODO: implement queries responding to early end index
		public int EarlyEndIndex = -1;

		// IDEA: method for calculating line length. Could be used for menu UI, showing the length differences between each path to the player. 
		// IDEA: method measure the distance between 2 indices on a line.
		// IDEA: method for measuring all paths available between 2 indices (which might be on different lines)

		// IDEA: use centerline to check if car is going the wrong direction. compare velocity direction (if magnitude is above a threshold) against direction from closest point to next?

		// name which is only used in the editor window at the moment
		public string Name = "";
		// at which line point index of its parent line this line forks away at
		public int StartIndex = 0;
		// the points that the bezier curves are calculated from
		public List<Vector3> ControlPoints = new List<Vector3>();
		// the points outputted from the bezier curve calculation. cached and serialized
		// IDEA: spatially partition line points for optimizing access and comparison operations, such as getting closest point on polyline. might require redundant caching
		public List<Vector3> LinePoints = new List<Vector3>();
		// how many snapshots are used for each generated bezier curve, this combined with the number of control points decides how many line points are generated
		public int Resolution = 10;

		// cached editor window fold state for keeping the foldout showing the child lines/forks opened or closes when deselecting and reselecting the line in the editor
		public bool ForksInspectorFoldState = false;
		// the child lines/forks, branches of the tree node. each line in this list is unique and is not referenced by any other fork lists.
		public List<InternalCenterline> Forks = new List<InternalCenterline>();

		// IDEA: 	no rejoin line, use forks instead, and limit forks to only be able to start at end of parent instead of anywhere on parent. 
		//			would solve problem of parent lines of lines containing the goalpost potentially leading players into (and locking them into) a dead end
		// IDEA: alternatively define an optional early end line index for each line which limits queries from searching past that index


		// cyclical tree node reference. can be null, meaning that the line ends in the air instead or rejoining any line
		// FIXME: rejoin line set to main line on deserialization(?)
		public InternalCenterline RejoinLine = null;
		// on what line point index of its rejoin line it connects. the referenced line point will be used as a control point
		public int RejoinIndex = 0;

		public void SetActiveRecursive(bool active) {
			Active = active;
			foreach (var fork in Forks)
				fork.SetActiveRecursive(active);
		}

		public void PopulateFlattenedTree(ref List<InternalCenterline> flatTree) {
			if (flatTree == null) flatTree = new List<InternalCenterline>();

			flatTree.Add(this);

			foreach (var fork in Forks)
				fork.PopulateFlattenedTree(ref flatTree);
		}
	}

	// serializable version of the class which is only used for saving the line to the scene
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

		// index to the serialized line in the flattened tree list which corresponds to the rejoin line (cyclical tree reference)
		public int indexOfRejoinLine;
		public int RejoinIndex;

	}

	// root of tree
	public InternalCenterline MainCenterline = new InternalCenterline();
	// cache of serialized flattened tree. is only used when saving. can be safely cleared or otherwise altered without affecting anything
	public List<SerializableInternalCenterline> SerializedLines;

	// TODO: generate co-driver calls based on angle delta of set distance ahead of closest point. will probably be implemented in a different class/monobehaviour for the UI, or in the steering script.
	// TODO: use centerline to pull car towards center of road as a handicap option. will probably be implemented in the steering script
	// TODO: use centerline as respawn when falling off track. will probably be implemented in the steering script

	// TODO: wire up goal posts to use centerline instead of road segments for catching skipping of goal posts and reversing into goal post
	// IDEA: do all lap queries and teleportation through the centerline system, instead of triggering it using collision with the goal post trigger colliders

	// TODO: method for getting closest point within defined index range ahead, or max distance ahead along curve

	// TODO: use centerline as cheat mitigation, resetting car to last valid point on line when skipping too far ahead
	// IDEA: provide delta time in delta position queries, calculate car speed and compare it against a set max allowed speed limit

	public float LineThickness = 1f;
	public Color ActiveLineColor = Color.white;
	public Color InactiveLineColor = Color.gray;

	// Custom serialization
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
		} else {
			serializedNode.indexOfRejoinLine = -1;
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

		if (serializedLine.indexOfRejoinLine < 0) {
			newLine.RejoinLine = null;
		} else {
			if (refCache.TryGetValue(serializedLine.indexOfRejoinLine, out InternalCenterline rejoinLine)) {
				newLine.RejoinLine = rejoinLine;
			} else {
				rejoinRefResolveQueue.Add((serializedLine.indexOfRejoinLine, newLine));
			}
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

		// UnityEditor.Handles.color = Color.white;

		DrawLine(MainCenterline);

	}

	// Draw the lines between the line points. recursive
	void DrawLine(InternalCenterline line, int depth = 0) {
		if (depth > MAX_DEPTH) {
			Debug.LogError("Gizmo line draw recursion too deep");
			return;
		}

		if (line.LinePoints.Count < 1)
			return;

		if (line.Active)
			UnityEditor.Handles.color = ActiveLineColor;
		else
			UnityEditor.Handles.color = InactiveLineColor;

		// for (int i = 1; i < line.LinePoints.Count; i++) {
		// 	UnityEditor.Handles.DrawLine(
		// 		transform.TransformPoint(line.LinePoints[i - 1]),
		// 		transform.TransformPoint(line.LinePoints[i]),
		// 		LineThickness
		// 	);
		// }

		UnityEditor.Handles.lighting = false;

		UnityEditor.Handles.DrawAAPolyLine(LineThickness,
			// UnityEditor.Handles.DrawPolyLine(
			line.LinePoints
				.Select(vector => transform.TransformPoint(vector))
				.ToArray()
		);

		foreach (var fork in line.Forks) {
			DrawLine(fork, depth + 1);
		}

	}
#endif

	// Calculate bezier line points from control points and resolution, populates line point list
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

		// TODO: implement ignoring inactive lines/forks

		if (depth > MAX_DEPTH) {
			Debug.LogError("Get rotation delta recursion too deep");
			yield break;
		}

		if (startIndex < 0) startIndex = 0;

		// Quaternion compareRotValue = compareRot ?? Quaternion.LookRotation(line.LinePoints[startIndex + 1] - line.LinePoints[startIndex], Vector3.up);

		float distanceTraveledSqr = 0;
		int lineEndIndex = line.EarlyEndIndex < 0 ? line.LinePoints.Count : line.EarlyEndIndex;
		for (int i = startIndex + 1; i < lineEndIndex; i++) {
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
			if (fork.StartIndex < startIndex || fork.StartIndex > line.LinePoints.Count)
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
	/// returns IEnumerable<(index at greates delta, which fork this result applies to, rotation delta)>
	public static IEnumerable<(int, InternalCenterline, Quaternion)> GetGreatestRotationDeltasAhead(
		InternalCenterline line,
		float distanceAheadSqr,
		int startIndex = 0,
		Quaternion? compareRot = null,
		int depth = 0
	) {

		// IDEA: search backwards if distance is negative?
		// IDEA: option to ignore some distance at the start in front of the car

		// IDEA: option to only return once with greatest delta of all forks

		// TODO: implement ignoring inactive lines/forks

		// abort if recursive call is too deep
		if (depth > MAX_DEPTH) {
			Debug.LogError("Get greatest rotation delta recursion too deep");
			yield break;
		}

		if (startIndex < 0) startIndex = 0;

		float distanceTraveledSqr = 0;
		Quaternion greatestDelta = Quaternion.identity;
		float greatestDeltaAngle = 0;
		int indexAtGreatestDelta = -1;

		// set rotation to compare against to be the start point on the first line in the call chain, meaning that child lines/forks also compare against this same rotation
		Quaternion compareRotValue = compareRot ?? Quaternion.LookRotation(line.LinePoints[startIndex + 1] - line.LinePoints[startIndex], Vector3.up);

		// step through the line points, from the given start point to the end of the line
		int lineEndIndex = line.EarlyEndIndex < 0 ? line.LinePoints.Count : line.EarlyEndIndex;
		for (int i = startIndex + 1; i < lineEndIndex; i++) {
			float distanceSqr = (line.LinePoints[i] - line.LinePoints[i - 1]).sqrMagnitude;

			// accumulate distance between line points to keep track of distance traveled since start index
			distanceTraveledSqr += distanceSqr;

			// direction from previous line point
			Quaternion outRot =
				Quaternion.LookRotation(line.LinePoints[i] - line.LinePoints[i - 1], Vector3.up)
				// * inverseRot
				;

			// get largest delta angle on any axis
			float angle = Quaternion.Angle(
				// Quaternion.identity, 
				compareRotValue,
				outRot
			);

			// check if new delta is largest found so far
			if (greatestDeltaAngle <= angle) {
				greatestDeltaAngle = angle;
				greatestDelta = outRot;
				indexAtGreatestDelta = i;
			}

			if (distanceTraveledSqr < distanceAheadSqr) {
				// continue searching for larger deltas if there is still distance left to search
				continue;
			} else {
				// return largest delta found if the end of the measurment distance has been reached
				yield return (indexAtGreatestDelta, line, greatestDelta);
				break;
			}
		}

		if (distanceTraveledSqr < distanceAheadSqr) {
			// return largest delta found if the end of the line has been found
			yield return (indexAtGreatestDelta, line, greatestDelta);

			if (line.RejoinLine != null) {
				// continue searching the line that this line rejoins if the end of the line is reached before the end of the search distance
				foreach (var rejoinResult in GetGreatestRotationDeltasAhead(line.RejoinLine, distanceAheadSqr - distanceTraveledSqr, line.RejoinIndex, compareRot, depth + 1))
					yield return rejoinResult;
			}
		}

		// check any forks whose starting/forking point is within the measuring distance
		foreach (var fork in line.Forks) {
			// check if forking point is after measurement starting point
			if (fork.StartIndex < startIndex)
				continue;

			// calculate how far into the next fork will be measured
			float forkDistanceAheadSqr = distanceAheadSqr;
			for (int i = startIndex + 1; i <= fork.StartIndex; i++) {
				float distanceSqr = (line.LinePoints[i] - line.LinePoints[i - 1]).sqrMagnitude; // NOTE: does not use transform scale, distance ahead is relative to internal point measurement
				forkDistanceAheadSqr -= distanceSqr;
			}

			// check if forking point is before end of measurement distance
			if (forkDistanceAheadSqr < 0)
				continue;

			foreach (var forkResult in GetGreatestRotationDeltasAhead(fork, forkDistanceAheadSqr, 0, compareRotValue, depth + 1))
				yield return forkResult;
		}

	}

	/// returns only the one point with the single largest rotation delta of all forks measured within the search distance
	public static (int, InternalCenterline, Quaternion) GetSingleGreatestRotationDelta(
		InternalCenterline line,
		float distanceAheadSqr,
		int startIndex = 0,
		Quaternion? compareRot = null
	) {

		(int, InternalCenterline, Quaternion) currentGreatest = (0, line, Quaternion.identity);

		float currentGreatestAngle = 0;

		foreach (var result in GetGreatestRotationDeltasAhead(line, distanceAheadSqr)) {
			if (Quaternion.Angle(Quaternion.identity, result.Item3) >= currentGreatestAngle)
				currentGreatest = result;
		}

		return currentGreatest;
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

		// TODO: implement ignoring inactive lines/forks
		
		pos = transform.InverseTransformPoint(pos);

		Vector3 currentClosest = Vector3.zero;
		float currentClosestDistance = 0;
		int lineIndex = 0;
		closestLine = line;


		int lineEndIndex = line.EarlyEndIndex < 0 ? line.LinePoints.Count : line.EarlyEndIndex;
		for (int i = 1; i < lineEndIndex; i++) {
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

	public void SetReachableActive(InternalCenterline fromLine, int fromIndex, InternalCenterline toLine, int toIndex, bool setActive = true, bool setInactive = false) {
		List<InternalCenterline> visited = new List<InternalCenterline>();

		// disable all lines
		if (setInactive)
			MainCenterline.SetActiveRecursive(false);

		SetReachableActive(visited, toLine, toIndex, fromLine, fromIndex, setActive, setInactive);

	}

	public bool SetReachableActive(List<InternalCenterline> visited, InternalCenterline toLine, int toIndex, InternalCenterline currentLine, int startIndex, bool setActive, bool setInactive) {


		if (visited.Contains(currentLine))
			return currentLine.Active;

		visited.Add(currentLine);
		currentLine.EarlyEndIndex = -1;

		if (currentLine == toLine) {
			// NOTE: might not catch reentry onto line at point past finishline?

			if (setActive)
				currentLine.Active = true;

			foreach (var fork in currentLine.Forks) {
				if (fork.StartIndex < toIndex)
					SetReachableActive(visited, toLine, toIndex, fork, 0, setActive, setInactive);
			}


			return true;
		}

		bool anyForkSucceeded = false;
		foreach (var fork in currentLine.Forks) {
			if (fork.StartIndex < startIndex)
				continue;

			bool success = SetReachableActive(visited, toLine, toIndex, fork, 0, setActive, setInactive);
			if (success) {
				if (currentLine.EarlyEndIndex < fork.StartIndex)
					currentLine.EarlyEndIndex = fork.StartIndex;
				anyForkSucceeded = true;
			}
		}

		// FIXME: not checking forks of self rejoining lines which start checking past the start of the forks but also rejoins before the start of the forks

		if (currentLine.RejoinLine != null) {
			if (currentLine.RejoinLine == toLine) {
				if (currentLine.RejoinIndex < toIndex) {
					if (setActive)
						currentLine.Active = true;
					return true;
				}

				if (setInactive)
					currentLine.Active = false;
				return false;
			}

			bool success = SetReachableActive(visited, toLine, toIndex, currentLine.RejoinLine, currentLine.RejoinIndex, setActive, setInactive);
			if (success) {
				currentLine.EarlyEndIndex = -1;
				anyForkSucceeded = true;
			}
		}


		if (anyForkSucceeded
		// || currentLine == toLine
		// || (currentLine.RejoinLine == toLine && currentLine.RejoinIndex < toIndex)
		) {
			if (setActive)
				currentLine.Active = true;

			return true;
		}

		// if (currentLine.RejoinLine == null) {
		// 	if (setInactive)
		// 		currentLine.Active = false;
		// 	return false;
		// }

		if (setInactive)
			currentLine.Active = false;
		return false;
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
