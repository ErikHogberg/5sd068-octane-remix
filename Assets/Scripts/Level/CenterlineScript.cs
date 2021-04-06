using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CenterlineScript : MonoBehaviour, ISerializationCallbackReceiver {

	const int MAX_DEPTH = 3;

	public class InternalCenterline {
		public int StartIndex = 0;
		public List<Vector3> ControlPoints = new List<Vector3>();
		// IDEA: spatially partition line points for optimizing access and comparison operations, such as getting closest point on polyline
		public List<Vector3> LinePoints = new List<Vector3>();
		public int Resolution = 10;
		// IDEA: define rejoin index for defining where the line will end, on what index on a line (along with fork index for said line). used for looping lines or rejoining forks
		public float BezierSplitExponent = 10f;

		[HideInInspector]
		public bool ForksInspectorFoldState = false;
		public List<InternalCenterline> Forks = new List<InternalCenterline>();

	}

	[Serializable]
	public class SerializableInternalCenterline {
		public int StartIndex = 0;
		public List<Vector3> ControlPoints = new List<Vector3>();
		// IDEA: spatially partition line points for optimizing access and comparison operations, such as getting closest point on polyline
		public List<Vector3> LinePoints = new List<Vector3>();
		public int Resolution = 10;
		// IDEA: define rejoin index for defining where the line will end, on what index on a line (along with fork index for said line). used for looping lines or rejoining forks
		public float BezierSplitExponent = 10f;

		[HideInInspector]
		public bool ForksInspectorFoldState = false;
		// public List<InternalCenterline> Forks = new List<InternalCenterline>();
		public int childCount;
		public int indexOfFirstChild;
	}


	public InternalCenterline MainCenterline = new InternalCenterline();
	// public List<InternalCenterline> Forks = new List<InternalCenterline>();

	public List<SerializableInternalCenterline> SerializedLines;// = new List<SerializableInternalCenterline>();

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

	void AddNodeToSerializedNodes(InternalCenterline n) {
		var serializedNode = new SerializableInternalCenterline() {
			StartIndex = n.StartIndex,
			ControlPoints = n.ControlPoints,
			LinePoints = n.LinePoints,
			Resolution = n.Resolution,
			BezierSplitExponent = n.BezierSplitExponent,
			ForksInspectorFoldState = n.ForksInspectorFoldState,
			// interestingValue = n.interestingValue,
			childCount = n.Forks.Count,
			indexOfFirstChild = SerializedLines.Count + 1
		};
		
		SerializedLines.Add(serializedNode);
		foreach (var child in n.Forks)
			AddNodeToSerializedNodes(child);
	}

	public void OnAfterDeserialize() {
		if (SerializedLines.Count > 0) {
			ReadNodeFromSerializedNodes(0, out MainCenterline);
		} else {
			MainCenterline = new InternalCenterline();
		}
	}

	int ReadNodeFromSerializedNodes(int index, out InternalCenterline node) {
		var serializedLine = SerializedLines[index];
		// Transfer the deserialized data into the internal Node class
		InternalCenterline newLine = new InternalCenterline() {
			StartIndex = serializedLine.StartIndex,
			ControlPoints = serializedLine.ControlPoints,
			LinePoints = serializedLine.LinePoints,
			Resolution = serializedLine.Resolution,
			BezierSplitExponent = serializedLine.BezierSplitExponent,
			ForksInspectorFoldState = serializedLine.ForksInspectorFoldState,
			// interestingValue = serializedNode.interestingValue,
			// children = new List<Node>()
			Forks = new List<InternalCenterline>()
		};

		// The tree needs to be read in depth-first, since that's how we wrote it out.
		for (int i = 0; i != serializedLine.childCount; i++) {
			InternalCenterline childNode;
			index = ReadNodeFromSerializedNodes(++index, out childNode);
			newLine.Forks.Add(childNode);
		}
		node = newLine;
		return index;
	}

#if UNITY_EDITOR
	void OnDrawGizmos() {

		UnityEditor.Handles.color = Color.white;

		DrawLine(MainCenterline);

		// for (int i = 1; i < MainCenterline.LinePoints.Count; i++) {
		// 	UnityEditor.Handles.DrawLine(
		// 		transform.TransformPoint(MainCenterline.LinePoints[i - 1]),
		// 		transform.TransformPoint(MainCenterline.LinePoints[i])
		// 	);
		// }

		// foreach (var fork in Forks) {
		// 	if (fork.LinePoints.Count < 1)
		// 		continue;

		// 	for (int i = 1; i < fork.LinePoints.Count; i++) {
		// 		UnityEditor.Handles.DrawLine(
		// 			transform.TransformPoint(fork.LinePoints[i - 1]),
		// 			transform.TransformPoint(fork.LinePoints[i])
		// 		);
		// 	}
		// }

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

	public static List<Vector3> GenerateLinePoints(Vector3? startControlPoint, List<Vector3> ControlPoints, int Resolution, float BezierSplitExponent = 10f) {
		List<Vector3> LinePoints = new List<Vector3>();

		int controlPointCount = ControlPoints.Count;

		// IEnumerable<Vector3> controlPoints = ControlPoints;

		if (startControlPoint is Vector3 startTemp) {
			ControlPoints = ControlPoints.Prepend(startTemp).ToList();
			controlPointCount++;
		}

		// Vector3 lastCurveEndDelta = Vector3.zero;

		int i = 0;
		while (i < controlPointCount) {
			int diff = controlPointCount - i;

			if (diff < 3) {
				if (i == 0) {
					if (controlPointCount == 1) {
						// return LinePoints;
						LinePoints.Add(ControlPoints[i]);
					}

					if (controlPointCount == 2) {
						LinePoints.Add(ControlPoints[i]);
						LinePoints.Add(ControlPoints[i + 1]);
					}

				} else {
					if (diff == 2) {
						Vector3 delta1 = ControlPoints[i] - ControlPoints[i - 1];
						// Vector3 delta2 = ControlPoints[i-1] - ControlPoints[i-2];
						Vector3 controlPoint1 = ControlPoints[i];
						Vector3 controlPoint2 = ControlPoints[i]
						 //  + delta1.normalized*delta2.magnitude;
						 + delta1;
						//lastCurveEndDelta.normalized * BezierSplitExponent;
						Vector3 controlPoint3 = controlPoint2;
						// Vector3 controlPoint3 = ControlPoints[i + 1];
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

				// lastCurveEndDelta = LinePoints[LinePoints.Count - 1] - LinePoints[LinePoints.Count - 2];
				// if (delta == Vector3.zero)
				// return;
				continue;
			}


			{
				// TODO: generate first point from direction of last 2 line points
				// IDEA: cache last 2 line points generated, or angle between them
				// IDEA: use previous control points delta instead
				Vector3 delta1 = ControlPoints[i] - ControlPoints[i - 1];
				// Vector3 delta2 = ControlPoints[i-1] - ControlPoints[i-2];
				Vector3 controlPoint1 = ControlPoints[i];
				Vector3 controlPoint2 = ControlPoints[i]
				 //  + delta1.normalized*delta2.magnitude;
				 + delta1;
				//lastCurveEndDelta.normalized * BezierSplitExponent;
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
				// lastCurveEndDelta = LinePoints[LinePoints.Count - 1] - LinePoints[LinePoints.Count - 2];
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

		line.LinePoints = GenerateLinePoints(lineStartPoint, line.ControlPoints, line.Resolution, line.BezierSplitExponent);

		foreach (var fork in line.Forks) {
			Vector3 forkStartPoint = line.LinePoints != null && line.LinePoints.Count > fork.StartIndex && fork.StartIndex >= 0 ? line.LinePoints[fork.StartIndex] : Vector3.zero;
			GenerateLinePoints(fork, forkStartPoint, depth + 1);
			// fork.LinePoints = GenerateLinePoints(null, fork.ControlPoints, fork.Resolution, fork.BezierSplitExponent);
			// fork.LinePoints = GenerateLinePoints(startPoint, fork.ControlPoints, fork.Resolution, fork.BezierSplitExponent);
		}

	}

	/// Gets all rotation deltas of paths ahead (current line + any new forks in the distance ahead), measured between closest point on line and point that is the given distance ahead
	public IEnumerable<(int, InternalCenterline, Quaternion)> GetRotationDeltaAhead(Vector3 pos, float distanceAhead) {
		Vector3 closestPos = GetClosestPoint(pos, out int index, out InternalCenterline line, out float distance);
		// return GetRotationDeltaAhead(closestPos, index, distanceAhead, forkIndex);
		return GetRotationDeltaAhead(line, distanceAhead, index);
	}

	/// (Index at end, fork index, rotation delta)
	public static IEnumerable<(int, InternalCenterline, Quaternion)> GetRotationDeltaAhead(InternalCenterline line, float distanceAhead, int startIndex = 0, Quaternion? compareRot = null, int depth = 0) {
		// IDEA: search backwards if distance is negative

		if (depth > MAX_DEPTH) {
			Debug.LogError("Get rotation delta recursion too deep");
			yield break;
		}

		Quaternion compareRotValue = compareRot ?? Quaternion.LookRotation(line.LinePoints[startIndex + 1] - line.LinePoints[startIndex], Vector3.up);

		float distanceAheadSqr = distanceAhead * distanceAhead;
		// int index = closestLineIndex;
		// Quaternion inverseRot = Quaternion.Inverse(Quaternion.LookRotation(MainCenterline.LinePoints[index + 1] - MainCenterline.LinePoints[index], Vector3.up));


		float distanceTraveledSqr = 0;
		for (int i = startIndex + 1; i < line.LinePoints.Count; i++) {
			float distanceSqr = (line.LinePoints[i] - line.LinePoints[i - 1]).sqrMagnitude;
			distanceTraveledSqr += distanceSqr;
			if (distanceTraveledSqr < distanceAheadSqr) {
				continue;
			} else {
				int compareLineIndex = i;

				Quaternion outRot = Quaternion.LookRotation(line.LinePoints[i] - line.LinePoints[i - 1], Vector3.up)
					// * inverseRot
					;

				yield return (compareLineIndex, line, outRot);
				break;
			}
		}

		foreach (var fork in line.Forks) {
			if (fork.StartIndex < startIndex)
				continue;

			// TODO: subtract distance between measure start and fork start
			// TODO: ignore forks that start beyond measurement distance
			float forkDistanceAhead = distanceAhead;
			foreach (var forkResult in GetRotationDeltaAhead(fork, forkDistanceAhead, 0, compareRotValue, depth + 1)) {
				yield return forkResult;
			}
		}

		// if (startFork < 0) {

		// 	// FIXME: distance ahead increases significantly as line resolution increases
		// 	float distanceTraveledSqr = 0;
		// 	for (int i = index + 1; i < MainCenterline.LinePoints.Count; i++) {
		// 		float distanceSqr = (MainCenterline.LinePoints[i] - MainCenterline.LinePoints[i - 1]).sqrMagnitude;
		// 		distanceTraveledSqr += distanceSqr;
		// 		if (distanceTraveledSqr < distanceAheadSqr) {
		// 			continue;
		// 		} else {
		// 			int compareLineIndex = i;

		// 			Quaternion outRot = Quaternion.LookRotation(MainCenterline.LinePoints[i] - MainCenterline.LinePoints[i - 1], Vector3.up)
		// 				* inverseRot;

		// 			yield return (compareLineIndex, -1, outRot);
		// 			break;
		// 		}
		// 	}

		// 	for (int forkIndex = 0; forkIndex < Forks.Count; forkIndex++) {
		// 		if (Forks[forkIndex].StartIndex <= index) {
		// 			continue;
		// 		}

		// 		distanceTraveledSqr = 0;

		// 		for (int i = index + 1; i < Forks[forkIndex].StartIndex; i++) {
		// 			float distanceSqr = (MainCenterline.LinePoints[i] - MainCenterline.LinePoints[i - 1]).sqrMagnitude;
		// 			distanceTraveledSqr += distanceSqr;
		// 		}

		// 		for (int i = index - Forks[forkIndex].StartIndex; i < Forks[forkIndex].LinePoints.Count; i++) {
		// 			if (i < 0) i = 0;
		// 			Vector3 prevPos = i <= 1 ? MainCenterline.LinePoints[Forks[forkIndex].StartIndex] : Forks[forkIndex].LinePoints[i - 1];
		// 			float distanceSqr = (Forks[forkIndex].LinePoints[i] - prevPos).sqrMagnitude;
		// 			distanceTraveledSqr += distanceSqr;
		// 			if (distanceTraveledSqr < distanceAheadSqr) {
		// 				continue;
		// 			} else {
		// 				// FIXME: returns fork first line pos one step on main line too early
		// 				if (i < 1) break;
		// 				Quaternion outRot = Quaternion.LookRotation(Forks[forkIndex].LinePoints[i] - prevPos, Vector3.up)
		// 					* inverseRot;

		// 				yield return (i, forkIndex, outRot);
		// 				break;
		// 			}
		// 		}
		// 	}
		// } else {
		// 	float distanceTraveledSqr = 0;

		// 	// FIXME: index out of bound at end, but only sometimes?
		// 	for (int i = index + 1; i < Forks[startFork].LinePoints.Count; i++) {
		// 		float distanceSqr = (Forks[startFork].LinePoints[i] - Forks[startFork].LinePoints[i - 1]).sqrMagnitude;
		// 		distanceTraveledSqr += distanceSqr;
		// 		if (distanceTraveledSqr < distanceAheadSqr) {
		// 			continue;
		// 		} else {
		// 			Quaternion outRot = Quaternion.LookRotation(Forks[startFork].LinePoints[i] - Forks[startFork].LinePoints[i - 1], Vector3.up)
		// 				* inverseRot;

		// 			yield return (i, startFork, outRot);
		// 			break;
		// 		}
		// 	}
		// }

		// compareLineIndex = -1;
		// yield return Quaternion.identity;
	}

	/// Gets all greatest rotation deltas of paths ahead (current line + any new forks in the distance ahead). 
	/// Measured between closest point on line and point that has the greatest rotation delta im the given distance ahead
	/// (Index at end, index at greates delta, fork index, rotation delta)
	public static IEnumerable<(int, int, InternalCenterline, Quaternion)> GetGreatestRotationDeltasAhead(InternalCenterline line, float distanceAhead, int startIndex = 0, Quaternion? compareRot = null, int depth = 0) {
		// IDEA: search backwards if distance is negative?
		// IDEA: option to ignore some distance at the start in front of the car

		// TODO: handle forks
		// IDEA: only return once with greatest delta of any fork
		// IDEA: return greatest delta of each fork
		// IDEA: bool to choose either of?

		if (depth > MAX_DEPTH) {
			Debug.LogError("Get greatest rotation delta recursion too deep");
			yield break;
		}

		// int index = closestLineIndex;
		float distanceAheadSqr = distanceAhead * distanceAhead;
		float distanceTraveledSqr = 0;
		Quaternion greatestDelta = Quaternion.identity;
		float greatestDeltaAngle = 0;
		int indexAtGreatestDelta = -1;

		// Quaternion inverseRot = Quaternion.Inverse(Quaternion.LookRotation(MainCenterline.LinePoints[index + 1] - MainCenterline.LinePoints[index], Vector3.up));
		// Quaternion inverseRot = Quaternion.Inverse(Quaternion.LookRotation(line.LinePoints[startIndex + 1] - line.LinePoints[startIndex], Vector3.up));

		Quaternion compareRotValue = compareRot ?? Quaternion.LookRotation(line.LinePoints[startIndex + 1] - line.LinePoints[startIndex], Vector3.up);

		// TODO: check forks too if on main centerline

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
			if (greatestDeltaAngle < angle) {
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

		foreach (var fork in line.Forks) {
			if (fork.StartIndex < startIndex)
				continue;

			// TODO: subtract distance between measure start and fork start
			// TODO: ignore forks that start beyond measurement distance
			float forkDistanceAhead = distanceAhead;
			foreach (var forkResult in GetGreatestRotationDeltasAhead(fork, forkDistanceAhead, 0, compareRotValue, depth + 1))
				yield return forkResult;
		}

		// if (closestForkIndex < 0) {
		// 	for (int i = index + 1; i < MainCenterline.LinePoints.Count; i++) {
		// 		float distanceSqr = (MainCenterline.LinePoints[i] - MainCenterline.LinePoints[i - 1]).sqrMagnitude;
		// 		distanceTraveledSqr += distanceSqr;
		// 		Quaternion outRot =
		// 			Quaternion.LookRotation(MainCenterline.LinePoints[i] - MainCenterline.LinePoints[i - 1], Vector3.up)
		// 			* inverseRot;

		// 		float angle = Quaternion.Angle(Quaternion.identity, outRot);
		// 		if (greatestDeltaAngle < angle) {
		// 			greatestDeltaAngle = angle;
		// 			greatestDelta = outRot;
		// 			indexAtGreatestDelta = i;
		// 		}

		// 		if (distanceTraveledSqr < distanceAheadSqr) {
		// 			continue;
		// 		} else {
		// 			yield return (i, indexAtGreatestDelta, -1, greatestDelta);
		// 			break;
		// 		}
		// 	}


		// for (int forkIndex = 0; forkIndex < Forks.Count; forkIndex++) {
		// 	if (Forks[forkIndex].StartIndex <= index) {
		// 		continue;
		// 	}

		// 	InternalCenterline line = Forks[forkIndex];
		// 	distanceTraveledSqr = 0;
		// 	greatestDeltaAngle = 0;

		// 	for (int i = index + 1; i < Forks[forkIndex].StartIndex; i++) {
		// 		float distanceSqr = (MainCenterline.LinePoints[i] - MainCenterline.LinePoints[i - 1]).sqrMagnitude;
		// 		distanceTraveledSqr += distanceSqr;
		// 	}

		// 	for (int i = index - Forks[forkIndex].StartIndex; i < line.LinePoints.Count; i++) {

		// 		if (i < 0) i = 0;
		// 		Vector3 prevPos = i <= 1 ? MainCenterline.LinePoints[Forks[forkIndex].StartIndex] : line.LinePoints[i - 1];

		// 		float distanceSqr = (line.LinePoints[i] - prevPos).sqrMagnitude;
		// 		distanceTraveledSqr += distanceSqr;

		// 		Vector3 lineDir = line.LinePoints[i] - prevPos;
		// 		if (lineDir == Vector3.zero)
		// 			continue;

		// 		Quaternion outRot =
		// 			Quaternion.LookRotation(lineDir, Vector3.up)
		// 			* inverseRot;

		// 		float angle = Quaternion.Angle(Quaternion.identity, outRot);
		// 		if (greatestDeltaAngle < angle) {
		// 			greatestDeltaAngle = angle;
		// 			greatestDelta = outRot;
		// 			indexAtGreatestDelta = i;
		// 		}

		// 		if (distanceTraveledSqr < distanceAheadSqr) {
		// 			continue;
		// 		} else {
		// 			if (i < 1) break;
		// 			yield return (i, indexAtGreatestDelta, forkIndex, greatestDelta);
		// 			break;
		// 		}
		// 	}

		// }


		// } else {
		// 	InternalCenterline line = Forks[closestForkIndex];

		// 	for (int i = index + 1; i < line.LinePoints.Count; i++) {
		// 		float distanceSqr = (line.LinePoints[i] - line.LinePoints[i - 1]).sqrMagnitude;
		// 		distanceTraveledSqr += distanceSqr;
		// 		Quaternion outRot =
		// 			Quaternion.LookRotation(line.LinePoints[i] - line.LinePoints[i - 1], Vector3.up)
		// 			* inverseRot;

		// 		float angle = Quaternion.Angle(Quaternion.identity, outRot);
		// 		if (greatestDeltaAngle < angle) {
		// 			greatestDeltaAngle = angle;
		// 			greatestDelta = outRot;
		// 			indexAtGreatestDelta = i;
		// 		}

		// 		if (distanceTraveledSqr < distanceAheadSqr) {
		// 			continue;
		// 		} else {
		// 			yield return (i, indexAtGreatestDelta, closestForkIndex, greatestDelta);
		// 			break;
		// 		}
		// 	}

		// }

		// return Quaternion.identity;
	}

	public Vector3 GetClosestPoint(Vector3 pos, out int closestLineIndex, out InternalCenterline closestLine, out float closestDistance, int depth = 0) {

		pos = transform.InverseTransformPoint(pos);

		Vector3 currentClosest = Vector3.zero;
		float currentClosestDistance = 0;
		int lineIndex = 0;
		// closestForkIndex = -1;
		closestLine = MainCenterline;



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

		if (depth > 10) {
			Debug.LogError("Get closest point recursion too deep");
			closestDistance = currentClosestDistance;
			closestLineIndex = lineIndex;
			return currentClosest;
		}

		// TODO: recursively check forks for closest
		// for (int forkIndex = 0; forkIndex < Forks.Count; forkIndex++) {
		// 	for (int i = 1; i < Forks[forkIndex].LinePoints.Count; i++) {
		// 		float distance = distanceToSegment(Forks[forkIndex].LinePoints[i - 1], Forks[forkIndex].LinePoints[i], pos);

		// 		if (distance < currentClosestDistance) {
		// 			currentClosestDistance = distance;
		// 			currentClosest = ProjectPointOnLineSegment(Forks[forkIndex].LinePoints[i - 1], Forks[forkIndex].LinePoints[i], pos);
		// 			lineIndex = i - 1;
		// 			closestForkIndex = forkIndex;
		// 		}
		// 	}
		// }

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
