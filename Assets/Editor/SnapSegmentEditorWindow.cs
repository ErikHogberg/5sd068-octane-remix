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


		GUILayout.BeginHorizontal();
		GUILayout.Label("Start:");
		startTransform = (StraightLevelPieceScript)EditorGUILayout.ObjectField(startTransform, typeof(StraightLevelPieceScript), true);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("End:");
		endTransform = (StraightLevelPieceScript)EditorGUILayout.ObjectField(endTransform, typeof(StraightLevelPieceScript), true);
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

		if (!middleSegment) {
			middleSegment = Selection.activeTransform.GetComponent<StraightLevelPieceScript>();
		}

		if (!middleSegment) {
			Debug.LogWarning("No road segment selected, road segment script not found in object");
			return;
		}

		// TODO: Evenly bend middle segment between start and end segment
		// TODO: interpolate width between start and end

		// IDEA: add 2 fields for inner and outer length override 

		float leftLength = 0;
		for (int i = 0; i < middleSegment.LeftBones.Count - 1; i++) {
			var currentBone = middleSegment.LeftBones[i];
			var nextBone = middleSegment.LeftBones[i + 1];

			leftLength += Vector3.Distance(currentBone.transform.position, nextBone.transform.position);

		}

		float distributedLeftLength = leftLength/middleSegment.LeftBones.Count;
		

		float rightLength = 0;
		for (int i = 0; i < middleSegment.RightBones.Count - 1; i++) {
			var currentBone = middleSegment.RightBones[i];
			var nextBone = middleSegment.RightBones[i + 1];

			rightLength += Vector3.Distance(currentBone.transform.position, nextBone.transform.position);
		}

		float distributedRightLength = rightLength/middleSegment.RightBones.Count;


		// TODO: check validity of bone indices
		float StartWidth = Vector3.Distance(StartSegment.FrontBones[0].transform.position, StartSegment.FrontBones[0].transform.position);
		float EndWidth = Vector3.Distance(EndSegment.RearBones[0].transform.position, EndSegment.RearBones[0].transform.position);

		// TODO: calculate angle/axis between start and end
		// IDEA: option to choose if inner and outer angle/axis should be independent or the same (using center?)

		// TODO: rotate bones of each side toward eachother		

	}
}
