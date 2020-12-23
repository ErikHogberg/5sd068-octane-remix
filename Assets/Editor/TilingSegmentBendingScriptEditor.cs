using UnityEditor;
using UnityEngine;
using System.Collections;

// Custom Editor using SerializedProperties.
// Automatic handling of multi-object editing, undo, and Prefab overrides.
[CustomEditor(typeof(TilingSegmentBendingScript))]
[CanEditMultipleObjects]
public class TilingSegmentBendingScriptEditor : Editor {
	// SerializedProperty cellSize;
	SerializedProperty oppositeCorner;
	SerializedProperty startMagnitude;
	SerializedProperty targetMagnitude;
	SerializedProperty segmentPrefab;

	void OnEnable() {
		// Setup the SerializedProperties.
		// cellSize = serializedObject.FindProperty("CellSize");
		oppositeCorner = serializedObject.FindProperty("Target");
		startMagnitude = serializedObject.FindProperty("StartMagnitude");
		targetMagnitude = serializedObject.FindProperty("TargetMagnitude");
		segmentPrefab = serializedObject.FindProperty("Segment");
	}

	private void OnSceneGUI() {
		TilingSegmentBendingScript segmentBendingScript = target as TilingSegmentBendingScript;

		Handles.color = Color.white;

		// TODO: handles for end of each segment
		// TODO: entries for each segment with options for if the curve is quadratic, cubic, uses only angle from previous segment, etc.
		foreach (var item in segmentBendingScript.segments) {

			Vector3 p0l = item.Item2.RearLeftBone.position;
			Vector3 p0r = item.Item2.RearRightBone.position;

			Vector3 lastBone = p0l;
			foreach (var bone in item.Item2.LeftBones) {
				// TODO: apply transform to line positions
				Handles.DrawLine(lastBone, bone.position);
			}

			lastBone = p0r;
			foreach (var bone in item.Item2.RightBones) {
				// TODO: apply transform to line positions
				Handles.DrawLine(lastBone, bone.position);
			}


			EditorGUI.BeginChangeCheck();
			Transform handleTransform = item.Item2.RearLeftBone;
			Quaternion handleRotation = item.Item2.RearLeftBone.rotation;
			p0l = Handles.DoPositionHandle(p0l, handleRotation);
			if (EditorGUI.EndChangeCheck()) {
				// Undo.RecordObject(line, "Move Point");
				// EditorUtility.SetDirty(line);
				item.Item2.RearLeftBone.position = handleTransform.InverseTransformPoint(p0l);
			}

			Vector3 p1l = item.Item2.FrontLeftBone.position;
			handleTransform = item.Item2.FrontLeftBone;
			handleRotation = item.Item2.FrontLeftBone.rotation;
			EditorGUI.BeginChangeCheck();
			p1l = Handles.DoPositionHandle(p1l, handleRotation);
			if (EditorGUI.EndChangeCheck()) {
				// Undo.RecordObject(line, "Move Point");
				// EditorUtility.SetDirty(line);
				// line.p1 = handleTransform.InverseTransformPoint(p1l);
				item.Item2.FrontLeftBone.position = handleTransform.InverseTransformPoint(p1l);
			}
		}

	}

	public override void OnInspectorGUI() {
		// Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
		serializedObject.Update();

		// oppositeCorner = serializedObject.FindProperty("OppositeCorner");
		// cellPrefab = serializedObject.FindProperty("CellPrefab");

		TilingSegmentBendingScript tilingBender = (TilingSegmentBendingScript)target;

		// Show the custom GUI controls.
		// EditorGUILayout.Slider(cellSize, 0, 100, new GUIContent("Cell Size"));
		// tilingBender.CellSize = EditorGUILayout.Slider(tilingBender.CellSize, 0.01f, 30);

		// Only show the damage progress bar if all the objects have the same damage value:
		// if (!cellSize.hasMultipleDifferentValues)
		// ProgressBar(cellSize.floatValue / 100.0f, "Cell Size");

		EditorGUILayout.PropertyField(oppositeCorner, new GUIContent("Opposite Corner"));
		EditorGUILayout.PropertyField(startMagnitude, new GUIContent("Start Magnitude"));
		EditorGUILayout.PropertyField(targetMagnitude, new GUIContent("End Magnitude"));
		EditorGUILayout.PropertyField(segmentPrefab, new GUIContent("Cell"));

		// GUILayout.Label("Cells: " + tilingBender.CellCount);
		if (GUILayout.Button("Clear grid")) {
			// tilingBender.ClearCells();
		}

		if (GUILayout.Button("Update grid")) {
			// tilingBender.UpdateCells();
		}

		// TODO: toggle for checking collision on cell locations, with dynamically hidden settings
		// tilingBender.CheckCollision = EditorGUILayout.Toggle("Collision", tilingBender.CheckCollision);
		// if(gridUpdater.CheckCollision){
		// 	gridUpdater.CollisionMask= EditorGUILayout.MaskField("",gridUpdater.CollisionMask.value);
		// }

		// tilingBender.PrintDebug = EditorGUILayout.Toggle("Print debug", tilingBender.PrintDebug);

		// Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();
	}

	// Custom GUILayout progress bar.
	void ProgressBar(float value, string label) {
		// Get a rect for the progress bar using the same margins as a textfield:
		Rect rect = GUILayoutUtility.GetRect(18, 18, "TextField");
		EditorGUI.ProgressBar(rect, value, label);
		EditorGUILayout.Space();
	}
}
