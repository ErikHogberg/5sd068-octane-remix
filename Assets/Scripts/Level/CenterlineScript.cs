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

	// Tree recursion limit
	public const int MAX_DEPTH = 10;

	private static CenterlineScript mainInstance = null;
	public static CenterlineScript MainInstance => mainInstance;
	public static bool IsInitialized => mainInstance != null;
	// public static float ResetDistanceStatic => IsInitialized ? mainInstance.ResetDistance : -1f;
	public static Transform MainInstanceTransform => mainInstance.transform;
	public static InternalCenterline Root => mainInstance != null ? mainInstance.MainCenterline : null;

	// non-serialized run-time data container
	public class InternalCenterline {

		// runtime-only, non-serialized active state which includes/excludes the line from queries
		public bool Active = true;
		// runtime-only, non-serialized line index which causes the part of the line past this index to be excluded from queries (disabled if index is below 0)
		public int EarlyEndIndex = -1;

		// IDEA: method for calculating line length. Could be used for menu UI, showing the length differences between each path to the player. 
		// IDEA: method measure the distance between 2 indices on a line.
		// IDEA: method for measuring all paths available between 2 indices (which might be on different lines)

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

		// cyclical tree node reference. can be null, meaning that the line ends in the air instead or rejoining any line
		public InternalCenterline RejoinLine = null;
		// on what line point index of its rejoin line it connects. the referenced line point will be used as a control point
		public int RejoinIndex = 0;

		// how far away car can be from the closest point on the line before triggering a reset
		public float ResetDistance = 50;


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

		// index to the serialized line in the flattened tree list which corresponds to the rejoin line (cyclical tree reference), if any
		public int indexOfRejoinLine;
		public int RejoinIndex;

		public float ResetDistance = 50;

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
	// IDEA: have a span of a few indices where a goal post collider is allowed to be hit, otherwise triggering the cheat mitigation system and resetting the car
	// IDEA: give time and/or score penalty if no goal post collision call was received before leaving the span, implying that the car missed the goal post without going too far off the track to reset

	// IDEA: use centerline to check if car is going the wrong direction. compare velocity direction (if magnitude is above a threshold) against direction from closest point to next?

	// IDEA: provide delta time in cheat mitigation queries, calculate car speed and compare it against a set max allowed speed limit to decide if a reset should happen

	// how far away car can be from the closest point on the line before triggering a reset
	// IDEA: give each line in the tree its own optional reset distance which overrides this tree-wide distance
	// public float ResetDistance = 0;

	// TODO: make queries respond to start and finish
	// IDEA: dont check backwards if finish line is found ahead
	// IDEA: dont chack backwards past start of finish
	// TODO: runtime objects for setting start and finish on line
	// IDEA: work similar to test script objects
	public InternalCenterline StartLine { get; private set; } = null;
	public int StartIndex { get; private set; } = -1;
	public InternalCenterline FinishLine { get; private set; } = null;
	public int FinishIndex { get; private set; } = -1;

	public string StartLineInfo => $"Start: {(StartLine == null ? "none" : StartLine.Name)}, {StartIndex}";
	public string FinishLineInfo => $"Finish: {(FinishLine == null ? "none" : FinishLine.Name)}, {FinishIndex}";

	public static bool HasStart => mainInstance != null && mainInstance.StartLine != null && mainInstance.StartIndex >= 0;
	public static bool HasFinish => mainInstance != null && mainInstance.FinishLine != null && mainInstance.FinishIndex >= 0;

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
			ResetDistance = n.ResetDistance,
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
			RejoinIndex = serializedLine.RejoinIndex,
			ResetDistance = serializedLine.ResetDistance,
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

		Gizmos.DrawIcon(transform.position, "roadarrow.png");

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

		// old intended style, but bad performance
		// for (int i = 1; i < line.LinePoints.Count; i++) {
		// 	UnityEditor.Handles.DrawLine(
		// 		transform.TransformPoint(line.LinePoints[i - 1]),
		// 		transform.TransformPoint(line.LinePoints[i]),
		// 		LineThickness
		// 	);
		// }

		UnityEditor.Handles.lighting = false;

		UnityEditor.Handles.DrawAAPolyLine(LineThickness, // looks weird, but works
														  // UnityEditor.Handles.DrawPolyLine( // cant set thickness?
			line.LinePoints
				.Select(vector => transform.TransformPoint(vector))
				.ToArray()
		);

		foreach (var fork in line.Forks) {
			DrawLine(fork, depth + 1);
		}

	}
#endif

	private void Awake() {
		mainInstance = this;
	}

	private void OnDestroy() {
		if (mainInstance == this)
			mainInstance = null;
	}

	// Calculate bezier line points from control points and resolution, populates line point list
	public static List<Vector3> GenerateLinePoints(List<Vector3> ControlPoints, int Resolution) {
		List<Vector3> LinePoints = new List<Vector3>();

		int controlPointCount = ControlPoints.Count;

		int i = 0;
		while (i < controlPointCount) {
			int diff = controlPointCount - i;

			if (diff < 3) {
				// manually add remainders
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

	// calls line point generation method on all forks recursively
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

	#region Rotation delta queries

	/// Gets all rotation deltas of paths ahead (current line + any new forks in the distance ahead)
	/// Measured between the direction of closest point on line towards the next point after it, and the direction of the given other point on the line towards the previous point behind it.
	/// (Index at end, line at end, rotation delta)
	public static IEnumerable<(int, InternalCenterline, Quaternion)> GetRotationDeltaAheadStatic(Vector3 pos, float distanceAhead, bool ignoreEarlyEnd = false, bool includeInactive = false) {
		if (mainInstance == null)
			yield break;
		else
			mainInstance.GetRotationDeltaAhead(pos, distanceAhead, ignoreEarlyEnd, includeInactive);

	}

	public IEnumerable<(int, InternalCenterline, Quaternion)> GetRotationDeltaAhead(Vector3 pos, float distanceAhead, bool ignoreEarlyEnd = false, bool includeInactive = false) {
		Vector3 closestPos = GetClosestPoint(pos, out int index, out InternalCenterline line, out float distance, includeInactive, ignoreEarlyEnd);
		// return GetRotationDeltaAhead(closestPos, index, distanceAhead, forkIndex);
		return GetRotationDeltasAhead(line, distanceAhead * distanceAhead, index, ignoreEarlyEnd, includeInactive);
	}

	public static IEnumerable<(int, InternalCenterline, Quaternion)> GetRotationDeltasAhead(
		InternalCenterline line,
		float distanceAheadSqr,
		int startIndex = 0,
		// Quaternion? compareRot = null, 
		bool ignoreEarlyEnd = false,
		bool includeInactive = false,
		int depth = 0
	) {

		// ignore deactivated lines
		// FIXME: ignores forks too?
		if (!includeInactive && !line.Active)
			yield break;

		if (depth > MAX_DEPTH) {
			Debug.LogError("Get rotation delta recursion too deep");
			yield break;
		}

		if (startIndex < 0) startIndex = 0;

		// Quaternion compareRotValue = compareRot ?? Quaternion.LookRotation(line.LinePoints[startIndex + 1] - line.LinePoints[startIndex], Vector3.up);

		float distanceTraveledSqr = 0;
		int lineEndIndex = (ignoreEarlyEnd || line.EarlyEndIndex < 0) ? line.LinePoints.Count : line.EarlyEndIndex;
		for (int i = startIndex + 1; i < lineEndIndex; i++) {

			// calculate distance from previous point
			// FIXME: distance measurement seemingly affected by resolution of line. same measurement distance reaches further aling lines with higher resolutions 
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

		if ((ignoreEarlyEnd || line.EarlyEndIndex < 0) && distanceTraveledSqr < distanceAheadSqr && line.RejoinLine != null) {
			foreach (var rejoinResult in GetRotationDeltasAhead(line.RejoinLine, distanceAheadSqr - distanceTraveledSqr, line.RejoinIndex, ignoreEarlyEnd, includeInactive, depth + 1))
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
				ignoreEarlyEnd,
				includeInactive,
				depth + 1
				)) {
				yield return forkResult;
			}
		}

	}


	/// Gets all of the *greatest* rotation deltas of the paths ahead (current line + any new forks in the distance ahead). 
	/// Measured between the direction of closest point on line towards the next point after it, and the direction of the given other point on the line towards the previous point behind it.
	/// returns IEnumerable<(index at greates delta, which fork this result applies to, rotation delta)>
	public static IEnumerable<(int, InternalCenterline, Quaternion)> GetGreatestRotationDeltasAheadStatic(
		Vector3 pos,
		float distanceAhead,
		bool ignoreEarlyEnd = false,
		bool includeInactive = false
	) {
		if (mainInstance == null)
			yield break;
		else
			mainInstance.GetGreatestRotationDeltasAhead(pos, distanceAhead, ignoreEarlyEnd, includeInactive);
	}

	public IEnumerable<(int, InternalCenterline, Quaternion)> GetGreatestRotationDeltasAhead(Vector3 pos, float distanceAhead, bool ignoreEarlyEnd = false, bool includeInactive = false) {
		Vector3 closestPos = GetClosestPoint(pos, out int index, out InternalCenterline line, out float distance, includeInactive, ignoreEarlyEnd);
		// return GetRotationDeltaAhead(closestPos, index, distanceAhead, forkIndex);
		return GetGreatestRotationDeltasAhead(line, distanceAhead * distanceAhead, index, ignoreEarlyEnd, includeInactive);
	}

	public static IEnumerable<(int, InternalCenterline, Quaternion)> GetGreatestRotationDeltasAhead(
		InternalCenterline line,
		float distanceAheadSqr,
		int startIndex = 0,
		bool ignoreEarlyEnd = false,
		bool includeInactive = false,
		Quaternion? compareRot = null,
		int depth = 0
	) {

		// ignore deactivated lines
		if (!includeInactive && !line.Active)
			yield break;

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
		compareRot = compareRot ?? Quaternion.LookRotation(line.LinePoints[startIndex + 1] - line.LinePoints[startIndex], Vector3.up);
		if (!compareRot.HasValue)
			yield break;
		Quaternion compareRotValue = compareRot.Value;

		// step through the line points, from the given start point to the end of the line
		int lineEndIndex = (ignoreEarlyEnd || line.EarlyEndIndex < 0) ? line.LinePoints.Count : line.EarlyEndIndex;
		for (int i = startIndex + 1; i < lineEndIndex; i++) {

			// FIXME: distance measurement seemingly affected by resolution of line. same measurement distance reaches further aling lines with higher resolutions 
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

		if ((ignoreEarlyEnd || line.EarlyEndIndex < 0) && distanceTraveledSqr < distanceAheadSqr) {
			// return largest delta found if the end of the line has been found
			yield return (indexAtGreatestDelta, line, greatestDelta);

			if (line.RejoinLine != null) {
				// continue searching the line that this line rejoins if the end of the line is reached before the end of the search distance
				foreach (var rejoinResult in GetGreatestRotationDeltasAhead(line.RejoinLine, distanceAheadSqr - distanceTraveledSqr, line.RejoinIndex, ignoreEarlyEnd, includeInactive, compareRot, depth + 1))
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

			foreach (var forkResult in GetGreatestRotationDeltasAhead(fork, forkDistanceAheadSqr, 0, ignoreEarlyEnd, includeInactive, compareRotValue, depth + 1))
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
	#endregion

	#region GetClosestPoint
	/// Gets the position of the closest line point on the line and any of its child lines/forks
	public static Vector3 GetClosestPointStatic(Vector3 pos,
		out int closestLineIndex,
		out InternalCenterline closestLine,
		out float closestDistance,
		bool ignoreEarlyEnd = false, bool includeInactive = false
	) {
		if (mainInstance == null) {
			closestLineIndex = -1;
			closestLine = null;
			closestDistance = -1;
			return pos;
		}

		return mainInstance.GetClosestPoint(pos, out closestLineIndex, out closestLine, out closestDistance, ignoreEarlyEnd, includeInactive);
	}

	public Vector3 GetClosestPoint(Vector3 pos, out int closestLineIndex, out InternalCenterline closestLine, out float closestDistance, bool ignoreEarlyEnd = false, bool includeInactive = false) {
		return transform.TransformPoint(GetClosestPoint(pos, MainCenterline, transform, out closestLineIndex, out closestLine, out closestDistance, ignoreEarlyEnd, includeInactive));
	}

	public static Vector3 GetClosestPoint(Vector3 pos, InternalCenterline line, Transform transform,
		out int closestLineIndex,
		out InternalCenterline closestLine,
		out float closestDistance,
		bool ignoreEarlyEnd = false,
		bool includeInactive = false,
		int depth = 0
	) {


		pos = transform.InverseTransformPoint(pos);

		Vector3 currentClosest = Vector3.zero;
		float currentClosestDistance = float.MaxValue;
		int lineIndex = 0;
		closestLine = line;


		// TODO: implement ignoring inactive lines/forks
		if (includeInactive || line.Active) {
			int lineEndIndex = (ignoreEarlyEnd || line.EarlyEndIndex < 0) ? line.LinePoints.Count : line.EarlyEndIndex;
			for (int i = 1; i < lineEndIndex; i++) {
				float distance = Line.distanceToSegment(line.LinePoints[i - 1], line.LinePoints[i], pos);

				// if (i == 1) {
				// 	currentClosestDistance = distance;
				// 	currentClosest = ProjectPointOnLineSegment(line.LinePoints[i - 1], line.LinePoints[i], pos);
				// 	lineIndex = 0;
				// 	continue;
				// }

				if (distance < currentClosestDistance) {
					currentClosestDistance = distance;
					currentClosest = Line.ProjectPointOnLineSegment(line.LinePoints[i - 1], line.LinePoints[i], pos);
					lineIndex = i - 1;
				}
			}
		}

		if (depth > MAX_DEPTH) {
			Debug.LogError("Get closest point recursion too deep");
			closestDistance = currentClosestDistance;
			closestLineIndex = lineIndex;
			return currentClosest;
		}

		foreach (var fork in line.Forks) {
			Vector3 forkClosestPos = GetClosestPoint(pos, fork, transform,
				out int forkClosestIndex,
				out InternalCenterline forkClosestLine,
				out float forkClosestDistance,
				ignoreEarlyEnd, includeInactive, depth
			);

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

	public Vector3 GetClosestPoint(Vector3 pos, bool ignoreEarlyEnd = false, bool includeInactive = false) {
		return GetClosestPoint(pos, out int _index, out InternalCenterline _line, out float _closestDistance, ignoreEarlyEnd, includeInactive);
	}

	public Vector3 GetClosestPoint(Vector3 pos, out int closestLineIndex, out InternalCenterline closestFork, bool ignoreEarlyEnd = false, bool includeInactive = false) {
		Vector3 outPos = GetClosestPoint(pos, out int index, out InternalCenterline line, out float _closestDistance, ignoreEarlyEnd, includeInactive);
		closestLineIndex = index;
		closestFork = line;
		return outPos;
	}

	public Vector3 GetClosestPoint(Vector3 pos, out float closestDistance, bool ignoreEarlyEnd = false, bool includeInactive = false) {
		Vector3 outPos = GetClosestPoint(pos, out int _index, out InternalCenterline _line, out float distance, ignoreEarlyEnd, includeInactive);
		closestDistance = distance;
		return outPos;
	}

	// Get closest point within the distance ahead of a defined line point
	public static Vector3 GetClosestPointWithinRangeToIndexStatic(
			Vector3 pos,

			InternalCenterline line,
			float distanceAheadSqr,
			out float closestDistance,
			out (int, InternalCenterline) closestLinePoint,
			int startIndex = 0,
			bool ignoreEarlyEnd = false,
			bool includeInactive = false

		) {
		if (mainInstance != null) {
			return MainInstanceTransform.TransformPoint(
				mainInstance.GetClosestPointWithinRangeToIndex(
					pos,
					line,
					distanceAheadSqr,
					out closestDistance,
					out closestLinePoint,
					ignoreEarlyEnd,
					includeInactive,
					startIndex
					)
				);
		} else {
			throw new UnityException("centerline singleton somehow not initialized");
		}
	}

	public Vector3 GetClosestPointWithinRangeToIndex(
		Vector3 pos,

		InternalCenterline line,
		float distanceAheadSqr,
		out float closestDistance,
		out (int, InternalCenterline) closestLinePoint,
		bool ignoreEarlyEnd = false,
		bool includeInactive = false,
		int startIndex = 0,
		int depth = 0
	) {

		// IDEA: also return bool of if finish line was passed

		Vector3 inverseGlobalPos = transform.InverseTransformPoint(pos);

		Vector3 currentClosest = Vector3.zero;
		float currentClosestDistance = float.MaxValue;
		int lineIndex = 0;
		int closestLineIndex = 0;
		var closestLine = line;

		float distanceTraveledSqr = 0;

		startIndex = startIndex < 0 ? 0 : startIndex;
		int endIndex = 0;

		if (includeInactive || line.Active) {
			int lineEndIndex = (ignoreEarlyEnd || line.EarlyEndIndex < 0 || line.EarlyEndIndex > line.LinePoints.Count) ? line.LinePoints.Count : line.EarlyEndIndex + 1;
			for (int i = startIndex + 1; i < lineEndIndex; i++) {
				float distance = Line.distanceToSegment(line.LinePoints[i - 1], line.LinePoints[i], inverseGlobalPos);

				distanceTraveledSqr += (line.LinePoints[i - 1] - line.LinePoints[i]).sqrMagnitude;
				endIndex = i;

				if (distance < currentClosestDistance) {
					currentClosestDistance = distance;
					currentClosest = Line.ProjectPointOnLineSegment(line.LinePoints[i - 1], line.LinePoints[i], inverseGlobalPos);
					lineIndex = i - 1;
				}
				if (distanceTraveledSqr > distanceAheadSqr)
					break;
			}
		}
		closestLineIndex = lineIndex;

		if (depth > MAX_DEPTH) {
			Debug.LogError("Get closest point recursion too deep");
			closestDistance = currentClosestDistance;
			closestLinePoint = (closestLineIndex, closestLine);
			return currentClosest;
		}

		if (line.RejoinLine != null && (ignoreEarlyEnd || line.EarlyEndIndex < 0) && distanceTraveledSqr < distanceAheadSqr) {
			// continue searching the line that this line rejoins if the end of the line is reached before the end of the search distance
			Vector3 rejoinClosestPos = GetClosestPointWithinRangeToIndex(pos, line.RejoinLine, distanceAheadSqr - distanceTraveledSqr,
				out float rejoinClosestDistance, out var rejoinClosetLinePoint,
				ignoreEarlyEnd, includeInactive, line.RejoinIndex, depth + 1
			);

			if (rejoinClosestDistance < currentClosestDistance) {
				currentClosestDistance = rejoinClosestDistance;
				currentClosest = rejoinClosestPos;
				closestLineIndex = rejoinClosetLinePoint.Item1;
				closestLine = rejoinClosetLinePoint.Item2;
			}
		}

		foreach (var fork in line.Forks) {

			if (fork.StartIndex < startIndex || fork.StartIndex > endIndex)
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

			Vector3 forkClosestPos = GetClosestPointWithinRangeToIndex(pos, fork,
				forkDistanceAheadSqr,
				out float forkClosestDistance,
				out (int, InternalCenterline) forkClosestLinePoint,
				ignoreEarlyEnd, includeInactive, depth
			);

			if (forkClosestDistance < currentClosestDistance) {
				currentClosestDistance = forkClosestDistance;
				currentClosest = forkClosestPos;
				closestLineIndex = forkClosestLinePoint.Item1;
				closestLine = forkClosestLinePoint.Item2;
				// closestLine = fork;
			}
		}


		closestDistance = currentClosestDistance;
		closestLinePoint = (closestLineIndex, closestLine);
		return currentClosest;
	}
	#endregion

	// returns all earlier fork starts on the line (along with how far behind (squared distance) the start index it was found), within the given distance behind the given start index
	// interprets the start index as the last index on the line if the input value is invalid
	// NOTE: does not search lines that rejoin this line, potentially allowing cheating in niche situations by backtracking when rejoining a line to enter another fork which starts before where player enters the line
	public static IEnumerable<(int, float)> GetForkStartsBehind(InternalCenterline line, float distanceBehind, int startIndex = -1) {
		int pointCount = line.LinePoints.Count;
		float distanceBehindSqr = distanceBehind * distanceBehind;

		if (startIndex < 0 || startIndex >= pointCount) startIndex = pointCount - 1;

		float distanceTraveledSqr = 0;

		var forksBeforeStart = line.Forks.Where(f => f.StartIndex <= startIndex); // dunno if this saves any iterations, or if the filter is re-evaluated on each access

		bool returnedAny = false;
		for (int i = startIndex; i > 0; i--) {
			float distanceToNext = (line.LinePoints[i - 1] - line.LinePoints[i]).sqrMagnitude;
			distanceTraveledSqr += distanceToNext;

			if (forksBeforeStart.Any(f => f.StartIndex == i)) {
				returnedAny = true;
				yield return (i, distanceTraveledSqr);
			}

			if (distanceTraveledSqr > distanceBehindSqr)
				break;

		}

		if (!returnedAny) {
			if (distanceTraveledSqr > distanceBehindSqr)
				yield return (startIndex, 0);
			else
				yield return (-1, distanceTraveledSqr);
			// yield return (-1, 0);
		}
	}

	// returns the earliest fork start on the line (along with how far behind (squared distance) the start index it was found), within the given distance behind the given start index
	// interprets the start index as the last index on the line if the input value is invalid
	// returns the start index if no fork start was found
	public static (int, float) GetEarliestForkStartBehind(InternalCenterline line, float distanceBehind, int startIndex = -1) {
		float distanceBehindFound = 0;
		int earliestForkStart = line.LinePoints.Count - 1;
		foreach ((var forkStart, float howFar) in GetForkStartsBehind(line, distanceBehind, startIndex)) {
			if (forkStart < earliestForkStart) {
				earliestForkStart = forkStart;
				distanceBehindFound = howFar;
			};
		}
		return (earliestForkStart, distanceBehindFound);
	}

	#region UI
	public static Vector2 GetUIArrowDir(Quaternion rot) {
		Vector3 direction = rot * Vector3.forward;
		Vector3 projection = Vector3.ProjectOnPlane(direction, Vector3.back);

		return new Vector2(projection.x, projection.y);
	}

	public Vector2 GetUIArrowDir(Quaternion fromRot, Quaternion toRot) {
		Quaternion rot = toRot * Quaternion.Inverse(fromRot);
		return GetUIArrowDir(rot);
	}
	#endregion

	#region GoalPost

	public Vector3 SetGoalPost(Vector3 pos, bool setStart, bool setFinish, out Quaternion lineDir) {

		Vector3 closetPos = GetClosestPoint(pos, out int closestLineIndex, out InternalCenterline closestLine, out float closestDistance, ignoreEarlyEnd: true, includeInactive: true);
		if (closestLine.LinePoints.Count < 2) {
			lineDir = Quaternion.identity;
			Debug.LogWarning("centerline set goal post: closest line has too few line points to estimate a tangent direction");
		} else {
			if (closestLineIndex > 0)
				lineDir = Quaternion.FromToRotation(Vector3.forward, closestLine.LinePoints[closestLineIndex - 1] - closestLine.LinePoints[closestLineIndex]);
			else
				lineDir = Quaternion.FromToRotation(Vector3.forward, closestLine.LinePoints[closestLineIndex] - closestLine.LinePoints[closestLineIndex + 1]);
		}

		if (setStart) {
			StartLine = closestLine;
			StartIndex = closestLineIndex;
		}

		if (setFinish) {
			FinishLine = closestLine;
			FinishIndex = closestLineIndex;
		}

		UpdateReachableActive();

		return closetPos;
	}

	public void SetGoalPost(InternalCenterline line, int index, bool setStart, bool setFinish, bool updateActiveForks) {
		if (line == null || index < 0)
			return;

		if (setStart) {
			StartLine = line;
			StartIndex = index;
		}

		if (setFinish) {
			FinishLine = line;
			FinishIndex = index;
		}

		if (updateActiveForks) UpdateReachableActive();
	}

	// NOTE: might be theoretically possible to false negative by pre and post not being same line as goal post while still being a lines before and after the correct line
	public bool CheckLapCross(InternalCenterline preLine, int preIndex, InternalCenterline postLine, int postIndex) {
		if (preLine == FinishLine) {
			if (preIndex > FinishIndex)
				return false;

			if (postLine == FinishLine.RejoinLine && preLine != postLine)
				return true;
			if (postLine == FinishLine && postIndex > FinishIndex)
				return true;

		} else {
			if (postLine == FinishLine) {
				return postIndex > FinishIndex;
			}
		}

		return false;
	}

	public static bool CheckLapCrossStatic(InternalCenterline preLine, int preIndex, InternalCenterline postLine, int postIndex) {
		if (!mainInstance)
			return false;

		return mainInstance.CheckLapCross(preLine, preIndex, postLine, postIndex);
	}


	public bool FinishLineInRange(Vector3 pos, float rangeSqr) {

		if (FinishLine == null || FinishIndex < 0)
			return false;

		return GetSqrDistance(transform.TransformPoint(FinishLine.LinePoints[FinishIndex]), pos) < rangeSqr;
	}

	public bool FinishLineInRange(InternalCenterline line, int index, float rangeSqr) {

		if (FinishLine == null || FinishIndex < 0)
			return false;

		float distanceTraveledSqr = 0;
		for (int i = index + 1; i < line.LinePoints.Count; i++) {

			float sqrDistance = (line.LinePoints[i] - line.LinePoints[i - 1]).sqrMagnitude;
			distanceTraveledSqr += sqrDistance;
			if (distanceTraveledSqr > rangeSqr) {
				return false;
			}
			if (line == FinishLine && index == FinishIndex)
				return true;

			foreach (var fork in line.Forks) {
				if (index == fork.StartIndex && FinishLineInRange(fork, 0, rangeSqr = distanceTraveledSqr)) {
					return true;
				}
			}
		}

		// return GetSqrDistance(transform.TransformPoint(FinishLine.LinePoints[FinishIndex]), pos) < rangeSqr;

		return false;
	}

	public static bool FinishLineInRangeStatic(Vector3 pos, float rangeSqr) {
		if (!mainInstance)
			return false;

		return mainInstance.FinishLineInRange(pos, rangeSqr);
	}
	public static bool FinishLineInRangeStatic(InternalCenterline line, int index, float rangeSqr) {
		if (!mainInstance)
			return false;

		return mainInstance.FinishLineInRange(line, index, rangeSqr);
	}

	public static (Vector3, Quaternion) GetStartPosRot() {
		if (!mainInstance)
			return (Vector3.zero, Quaternion.identity);

		var startPos = MainInstanceTransform.TransformPoint(mainInstance.StartLine.LinePoints[mainInstance.StartIndex]);
		var startRot = mainInstance.StartIndex > 0 ?
			Quaternion.LookRotation(mainInstance.StartLine.LinePoints[mainInstance.StartIndex] - mainInstance.StartLine.LinePoints[mainInstance.StartIndex - 1], Vector3.up)
			:
			Quaternion.LookRotation(mainInstance.StartLine.LinePoints[mainInstance.StartIndex + 1] - mainInstance.StartLine.LinePoints[mainInstance.StartIndex], Vector3.up);
		startRot *= MainInstanceTransform.rotation;

		return (startPos, startRot);
	}

	public void UpdateReachableActive() {

		InternalCenterline startLine = StartLine;
		InternalCenterline finishLine = FinishLine;
		int startIndex = StartIndex;
		int finishIndex = FinishIndex;

		if (startLine == null) {
			startLine = FinishLine;
			startIndex = FinishIndex;
		}
		if (startLine == null) {
			startLine = MainCenterline;
			startIndex = 0;
		}
		if (startIndex < 0) startIndex = 0;

		if (finishLine == null) {
			finishLine = StartLine;
			finishIndex = StartIndex;
		}
		if (finishLine == null) {
			finishLine = MainCenterline;
			finishIndex = 0;
		}
		if (finishIndex < 0) finishIndex = 0;

		SetReachableActive(startLine, startIndex, finishLine, finishIndex);
	}

	public void SetReachableActive(InternalCenterline fromLine, int fromIndex, InternalCenterline toLine, int toIndex, bool setActive = true, bool setInactive = true) {
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
		) {
			if (setActive)
				currentLine.Active = true;

			return true;
		}

		if (setInactive)
			currentLine.Active = false;
		return false;
	}

	public static void InitProgressScript(CenterlineProgressScript progressScript) {
		if (!mainInstance || !progressScript) {
			Debug.LogWarning("centerline instance or progress script not assigned!");
			return;
		}

		if (mainInstance.StartLine != null)
			progressScript.SetLastValid(mainInstance.StartLine, mainInstance.StartIndex);
		else if (mainInstance.FinishLine != null)
			progressScript.SetLastValid(mainInstance.FinishLine, mainInstance.FinishIndex);
	}

	#endregion

	private static float GetSqrDistance(Vector3 a, Vector3 b) {
		return (a - b).sqrMagnitude;
	}

	public static float GetSqrDistance(InternalCenterline line, int startIndex, int endIndex) {
		if (startIndex == endIndex)
			return 0;
		if (startIndex > endIndex) {
			int tempIndex = startIndex;
			startIndex = endIndex;
			endIndex = tempIndex;
		}

		float distanceTraveledSqr = 0;
		for (int i = startIndex + 1; i <= endIndex; i++) {
			float sqrDistance = (line.LinePoints[i] - line.LinePoints[i - 1]).sqrMagnitude;
			distanceTraveledSqr += sqrDistance;
		}

		return distanceTraveledSqr;

		// return (a - b).sqrMagnitude;
	}

}
