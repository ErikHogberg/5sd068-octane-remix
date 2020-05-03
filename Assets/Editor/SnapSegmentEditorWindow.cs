using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

class SnapSegmentsEditorWindow : EditorWindow {

	// Transform target;

	// bool rotate = true;
	// bool scale = false;
	// Vector3 rotationOffset = Vector3.zero;

	Transform startSegmentObject;
	Transform endSegmentObject;

	Transform debugLeftStart;
	Transform debugLeftEnd;
	Transform debugRightStart;
	Transform debugRightEnd;


	private float leftStartBezierMagnitude;
	private float leftEndBezierMagnitude;
	private float rightStartBezierMagnitude;
	private float rightEndBezierMagnitude;

	[MenuItem("Window/Snap segments")]
	public static void ShowWindow() {
		EditorWindow.GetWindow(typeof(SnapSegmentsEditorWindow));
	}

	void OnGUI() {
		// GUILayout.BeginHorizontal();
		GUILayout.Label("Segment snapping tool!");
		// GUILayout.Label("Select 3 segments:");
		// GUILayout.Label("	start, end and then middle.");
		// GUILayout.EndHorizontal();


		// GUILayout.BeginHorizontal();
		GUILayout.Label("Start:");
		startSegmentObject = (Transform)EditorGUILayout.ObjectField(startSegmentObject, typeof(Transform), true);
		// GUILayout.EndHorizontal();
		// GUILayout.BeginHorizontal();
		GUILayout.Label("End:");
		endSegmentObject = (Transform)EditorGUILayout.ObjectField(endSegmentObject, typeof(Transform), true);
		// GUILayout.EndHorizontal();

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

		// leftDistance = EditorGUILayout.FloatField("Left distance:", leftDistance);
		// rightDistance = EditorGUILayout.FloatField("Right distance:", rightDistance);


		GUILayout.Space(16);

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Snap!", GUILayout.Width(165), GUILayout.Height(32))) {
			SnapSegments();
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		debugLeftStart = (Transform)EditorGUILayout.ObjectField(debugLeftStart, typeof(Transform), true);
		debugLeftEnd = (Transform)EditorGUILayout.ObjectField(debugLeftEnd, typeof(Transform), true);
		debugRightStart = (Transform)EditorGUILayout.ObjectField(debugRightStart, typeof(Transform), true);
		debugRightEnd = (Transform)EditorGUILayout.ObjectField(debugRightEnd, typeof(Transform), true);


	}

	void SnapSegments() {
		// var selection = Selection.GetTransforms(
		// 	SelectionMode.TopLevel | SelectionMode.Editable
		// );

		// if (selection.Length != 3) {
		// 	Debug.LogWarning("Wrong number of segments");
		// 	return;
		// }

		// List<StraightLevelPieceScript> straightSegments = selection.Select(s => s.GetComponentInChildren<StraightLevelPieceScript>()).ToList();
		// if (straightSegments.Any(s => !s)) {
		// 	Debug.LogWarning("1 or more objects aren't road segments");
		// 	return;
		// }

		// var StartSegment = straightSegments[0];
		// var EndSegment = straightSegments[1];
		// var MiddleSegment = straightSegments[2];

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

		// if (leftDistance < leftEndpointsDistance)
		// 	leftDistance = leftEndpointsDistance;
		// if (rightDistance < rightEndpointsDistance)
		// 	rightDistance = rightEndpointsDistance;

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

			if (debugLeftStart)
				debugLeftStart.position = leftStartDir;
			if (debugLeftEnd)
				debugLeftEnd.position = leftEndDir;
		}

		List<Vector3> rightPoints;
		{
			Vector3 rightStart = startSegment.FrontRightBone.position;
			Vector3 rightStartDir = rightStart - startSegment.FrontRightBone.up.normalized * rightStartBezierMagnitude;
			Vector3 rightEnd = endSegment.RearRightBone.position;
			Vector3 rightEndDir = rightEnd + endSegment.RearRightBone.right.normalized * rightEndBezierMagnitude;
			rightPoints = Bezier.CubicBezierRender(rightStart, rightStartDir, rightEndDir, rightEnd, middleSegment.RightBones.Count);

			if (debugRightStart)
				debugRightStart.position = rightStartDir;
			if (debugRightEnd)
				debugRightEnd.position = rightEndDir;
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
				var leftBone = middleSegment.LeftBones[i];
				var rightBone = middleSegment.RightBones[i];

				leftBone.LookAt(rightBone);
				leftBone.Rotate(Vector3.up * -90, Space.Self);
				leftBone.Rotate(Vector3.right * -90, Space.Self);
				rightBone.rotation = leftBone.rotation;

				// Quaternion leftBoneDir = Quaternion.FromToRotation(leftBone.forward, leftBone.position - rightBone.position);
				// Quaternion rightBoneDir = leftBoneDir;//Quaternion.FromToRotation(rightBone.position, leftBone.position) * Quaternion.Euler(0,90,0);



				// leftBone.rotation = leftBoneDir;
				// rightBone.rotation = rightBoneDir;

			}
		}

	}
}
