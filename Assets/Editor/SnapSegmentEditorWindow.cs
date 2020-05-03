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

	StraightLevelPieceScript StartSegment;
	StraightLevelPieceScript EndSegment;

	private float leftDistance;
	private float rightDistance;

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
		StartSegment = (StraightLevelPieceScript)EditorGUILayout.ObjectField(StartSegment, typeof(StraightLevelPieceScript), true);
		// GUILayout.EndHorizontal();
		// GUILayout.BeginHorizontal();
		GUILayout.Label("End:");
		EndSegment = (StraightLevelPieceScript)EditorGUILayout.ObjectField(EndSegment, typeof(StraightLevelPieceScript), true);
		// GUILayout.EndHorizontal();

		leftDistance = EditorGUILayout.FloatField("Left distance:", leftDistance);
		rightDistance = EditorGUILayout.FloatField("Right distance:", rightDistance);


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

		float StartWidth = Vector3.Distance(
			StartSegment.FrontRightBone.position,
			StartSegment.FrontLeftBone.transform.position
		);
		float EndWidth = Vector3.Distance(
			EndSegment.RearLeftBone.position,
			EndSegment.RearRightBone.position
		);

		float leftEndpointsDistance = Vector3.Distance(
			middleSegment.FrontLeftBone.position,
			middleSegment.RearRightBone.position
		);

		float rightEndpointsDistance = Vector3.Distance(
			middleSegment.FrontRightBone.position,
			middleSegment.RearLeftBone.position
		);

		// FIXME: somehow endpoint distances evaluate to the same?
		if (leftDistance < leftEndpointsDistance)
			leftDistance = leftEndpointsDistance;
		if (rightDistance < rightEndpointsDistance)
			rightDistance = rightEndpointsDistance;

		Undo.RecordObjects(middleSegment.LeftBones.ToArray(), "move left bones");
		Undo.RecordObjects(middleSegment.RightBones.ToArray(), "move right bones");

		Undo.RecordObject(middleSegment.FrontRightBone, "front right bone");
		Undo.RecordObject(middleSegment.FrontLeftBone, "front left bone");

		Undo.RecordObject(middleSegment.RearRightBone, "rear right bone");
		Undo.RecordObject(middleSegment.RearLeftBone, "rear left bone");

		Undo.RecordObject(middleSegment.FrontParent, "front bone parent");
		Undo.RecordObject(middleSegment.RearParent, "rear bone parent");

		// TODO: Evenly bend middle segment between start and end segment
		// TODO: interpolate width between start and end


		// TODO: calculate angle/axis between start and end
		// IDEA: option to choose if inner and outer angle/axis should be independent or the same (using center?)

		// TODO: rotate bones of each side toward eachother
		// TODO: scale

		// TODO: move root transform

		middleSegment.FrontParent.position = EndSegment.RearParent.position;
		middleSegment.FrontParent.rotation = EndSegment.RearParent.rotation;

		middleSegment.RearParent.position = StartSegment.FrontParent.position;
		middleSegment.RearParent.rotation = StartSegment.FrontParent.rotation;


		float leftSpacing = leftEndpointsDistance / middleSegment.LeftBones.Count;
		float rightSpacing = rightEndpointsDistance / middleSegment.RightBones.Count;

		Vector3 midpointUp = Quaternion.Slerp(
			Quaternion.Euler(StartSegment.FrontParent.up),
			Quaternion.Euler(EndSegment.RearParent.up),
			.5f
		).eulerAngles;


		float leftMidpointAngle = Mathf.Acos((leftEndpointsDistance * .5f) / leftDistance);
		float rightMidpointAngle = Mathf.Acos((rightEndpointsDistance * .5f) / rightDistance);

		Vector3 pos =
			(Quaternion.Euler(EndSegment.RearRightBone.forward)
			* Quaternion.AngleAxis(180 + leftMidpointAngle * Mathf.Rad2Deg, midpointUp))
			.eulerAngles
			* leftDistance;

		// middleSegment.LeftBones[5].position = EndSegment.RearRightBone.position + pos; // borked

		// for (int i = 0; i < middleSegment.RightBones.Count; i++) {

		// }
		// for (int i = 0; i < middleSegment.LeftBones.Count; i++) {

		// }

	}
}
