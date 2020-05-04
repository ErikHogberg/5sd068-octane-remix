using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

class SnapSegmentsEditorWindow : EditorWindow {

	Transform startSegmentObject;
	Transform endSegmentObject;

	private float leftStartBezierMagnitude;
	private float leftEndBezierMagnitude;
	private float rightStartBezierMagnitude;
	private float rightEndBezierMagnitude;

	[MenuItem("Window/Snap segments")]
	public static void ShowWindow() {
		EditorWindow.GetWindow(typeof(SnapSegmentsEditorWindow));
	}

	void OnGUI() {

		GUILayout.Label("Segment snapping tool!");

		GUILayout.Label("Start:");
		startSegmentObject = (Transform)EditorGUILayout.ObjectField(startSegmentObject, typeof(Transform), true);
		GUILayout.Label("End:");
		endSegmentObject = (Transform)EditorGUILayout.ObjectField(endSegmentObject, typeof(Transform), true);

		GUILayout.BeginHorizontal();
		GUILayout.Label("Left start magnitude:\t");
		leftStartBezierMagnitude = EditorGUILayout.FloatField(leftStartBezierMagnitude, GUILayout.Width(75));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Left end magnitude:\t");
		leftEndBezierMagnitude = EditorGUILayout.FloatField(leftEndBezierMagnitude, GUILayout.Width(75));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label("Right start magnitude:\t");
		rightStartBezierMagnitude = EditorGUILayout.FloatField(rightStartBezierMagnitude, GUILayout.Width(75));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Right end magnitude:\t");
		rightEndBezierMagnitude = EditorGUILayout.FloatField(rightEndBezierMagnitude, GUILayout.Width(75));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.Space(16);

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Snap!", GUILayout.Width(165), GUILayout.Height(32))) {
			SnapSegments();
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

	}

	void SnapSegments() {

		if (!Selection.activeTransform) {
			Debug.LogWarning("No object selected");
			return;
		}

		var middleSegment = Selection.activeTransform.GetComponentInChildren<StraightLevelPieceScript>();
		if (!middleSegment)
			middleSegment = Selection.activeTransform.GetComponent<StraightLevelPieceScript>();
		if (!middleSegment)
			middleSegment = Selection.activeTransform.parent.GetComponentInChildren<StraightLevelPieceScript>();
		if (!middleSegment) {
			Debug.LogWarning("No road segment selected, road segment script not found in object");
			return;
		}

		var startSegment = startSegmentObject.GetComponentInChildren<StraightLevelPieceScript>();
		if (!startSegment)
			startSegment = startSegmentObject.GetComponent<StraightLevelPieceScript>();
		if (!startSegment)
			startSegment = startSegmentObject.parent.GetComponentInChildren<StraightLevelPieceScript>();
		if (!startSegment) {
			Debug.LogWarning("Road segment script not found in start object");
			return;
		}

		var endSegment = endSegmentObject.GetComponentInChildren<StraightLevelPieceScript>();
		if (!endSegment)
			endSegment = endSegmentObject.GetComponent<StraightLevelPieceScript>();
		if (!endSegment)
			endSegment = endSegmentObject.parent.GetComponentInChildren<StraightLevelPieceScript>();
		if (!endSegment) {
			Debug.LogWarning("Road segment script not found in end object");
			return;
		}


		float StartWidth = Vector3.Distance(
			startSegment.FrontRightBone.position,
			startSegment.FrontLeftBone.transform.position
		);
		float EndWidth = Vector3.Distance(
			endSegment.RearLeftBone.position,
			endSegment.RearRightBone.position
		);

		float leftEndpointsDistance = Vector3.Distance(
			startSegment.FrontLeftBone.position,
			endSegment.RearRightBone.position
		);

		float rightEndpointsDistance = Vector3.Distance(
			startSegment.FrontRightBone.position,
			endSegment.RearLeftBone.position
		);

		Vector3 centerMidpoint = Vector3.Lerp(startSegment.FrontParent.position, endSegment.RearParent.position, .5f);

		Undo.RecordObjects(middleSegment.LeftBones.ToArray(), "move left bones");
		Undo.RecordObjects(middleSegment.RightBones.ToArray(), "move right bones");

		Undo.RecordObject(middleSegment.FrontRightBone, "front right bone");
		Undo.RecordObject(middleSegment.FrontLeftBone, "front left bone");

		Undo.RecordObject(middleSegment.RearRightBone, "rear right bone");
		Undo.RecordObject(middleSegment.RearLeftBone, "rear left bone");

		Undo.RecordObject(middleSegment.FrontParent, "front bone parent");
		Undo.RecordObject(middleSegment.RearParent, "rear bone parent");

		Undo.RecordObject(middleSegment.transform.parent.parent, "move segment root");

		// TODO: scale each bone length (only 1 axis) to match distance to next bone
		// TODO: evenly scale width and height of bone, interpolating between start- and end-point scale

		// Move root
		middleSegment.transform.parent.parent.position = centerMidpoint;

		middleSegment.FrontParent.position = endSegment.RearParent.position;
		middleSegment.FrontParent.rotation = endSegment.RearParent.rotation;

		middleSegment.RearParent.position = startSegment.FrontParent.position;
		middleSegment.RearParent.rotation = startSegment.FrontParent.rotation;


		float leftSpacing = leftEndpointsDistance / middleSegment.LeftBones.Count;
		float rightSpacing = rightEndpointsDistance / middleSegment.RightBones.Count;

		Vector3 midpointUp = Quaternion.Slerp(
			Quaternion.Euler(startSegment.FrontParent.forward),
			Quaternion.Euler(endSegment.RearParent.forward),
			.5f
		).eulerAngles;

		List<Vector3> leftPoints;
		{
			Vector3 leftStart = startSegment.FrontLeftBone.position;
			Vector3 leftStartDir = leftStart - startSegment.FrontLeftBone.up.normalized * leftStartBezierMagnitude;
			Vector3 leftEnd = endSegment.RearLeftBone.position;
			Vector3 leftEndDir = leftEnd - endSegment.RearLeftBone.right.normalized * leftEndBezierMagnitude;
			leftPoints = Bezier.CubicBezierRender(leftStart, leftStartDir, leftEndDir, leftEnd, middleSegment.LeftBones.Count);
		}

		List<Vector3> rightPoints;
		{
			Vector3 rightStart = startSegment.FrontRightBone.position;
			Vector3 rightStartDir = rightStart - startSegment.FrontRightBone.up.normalized * rightStartBezierMagnitude;
			Vector3 rightEnd = endSegment.RearRightBone.position;
			Vector3 rightEndDir = rightEnd + endSegment.RearRightBone.right.normalized * rightEndBezierMagnitude;
			rightPoints = Bezier.CubicBezierRender(rightStart, rightStartDir, rightEndDir, rightEnd, middleSegment.RightBones.Count);
		}

		// IDEA: separate script/fn for updating bone length and rotation according to distance to next bone


		if (leftPoints.Count != middleSegment.LeftBones.Count || rightPoints.Count != middleSegment.RightBones.Count) {
			Debug.LogError("bone and bezier point count dont match");
			return;
		}

		for (int i = 0; i < leftPoints.Count; i++) {
			middleSegment.LeftBones[leftPoints.Count - i - 1].position = leftPoints[i];
		}

		for (int i = 0; i < rightPoints.Count; i++) {
			middleSegment.RightBones[rightPoints.Count - i - 1].position = rightPoints[i];
		}

		if (middleSegment.LeftBones.Count == middleSegment.RightBones.Count) {
			for (int i = 0; i < middleSegment.LeftBones.Count; i++) {

				Transform nextLeftBone;
				Transform nextRightBone;
				if (i == middleSegment.LeftBones.Count - 1) {
					nextLeftBone = middleSegment.RearLeftBone;
					nextRightBone = middleSegment.RearRightBone;
				} else {
					nextLeftBone = middleSegment.LeftBones[i + 1];
					nextRightBone = middleSegment.RightBones[i + 1];
				}

				var leftBone = middleSegment.LeftBones[i];
				var rightBone = middleSegment.RightBones[i];

				leftBone.LookAt(nextLeftBone, (leftBone.position - rightBone.position).normalized);
				leftBone.Rotate(Vector3.right * 90, Space.Self);
				leftBone.Rotate(Vector3.up * -90, Space.Self);

				rightBone.LookAt(nextRightBone, (leftBone.position - rightBone.position).normalized);
				rightBone.Rotate(Vector3.right * 90, Space.Self);
				rightBone.Rotate(Vector3.up * -90, Space.Self);

			}
		}

	}
}
