using UnityEditor;
using UnityEngine;
using System.Collections;

// Custom Editor using SerializedProperties.
// Automatic handling of multi-object editing, undo, and Prefab overrides.
[CustomEditor(typeof(CenterlineScript))]
[CanEditMultipleObjects]
public class CenterlineEditorScript : Editor {
	SerializedProperty controlPoints;

	private bool autoUpdate = true;

	void OnEnable() {
		controlPoints = serializedObject.FindProperty("ControlPoints");
	}

	private void OnSceneGUI() {

		CenterlineScript centerlineScript = (CenterlineScript)target;

		Handles.color = Color.white;

		// TODO: draw lines along sides of grid
		// Handles.DrawLine(gridUpdater.transform.position, gridUpdater.);
		// Handles.DrawLine(gridUpdater.transform.position, gridUpdater.);
		// Handles.DrawLine(gridUpdater.transform.position, gridUpdater.);
		// Handles.DrawLine(gridUpdater.transform.position, gridUpdater.);

		/*
		{

			EditorGUI.BeginChangeCheck();
			Transform handleTransform = gridUpdater.transform;
			Quaternion handleRotation = gridUpdater.transform.rotation;
			Vector3 pos = handleTransform.TransformPoint(gridUpdater.CornerPos);
			pos = Handles.DoPositionHandle(pos, handleRotation);
			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(gridUpdater, "Move grid corner");
				EditorUtility.SetDirty(gridUpdater);
				// item.Item2.RearLeftBone.position = handleTransform.InverseTransformPoint(p0l);
				gridUpdater.CornerPos = handleTransform.InverseTransformPoint(pos);
				if (autoUpdate)
					gridUpdater.UpdateCells();
			}
		}

		switch (gridUpdater.RotationMode) {
			case GridUpdaterScript.CellRotationMode.OrientWithParent:
				break;
			case GridUpdaterScript.CellRotationMode.CustomUpDirection:
			case GridUpdaterScript.CellRotationMode.CustomGlobalRotation:
				EditorGUI.BeginChangeCheck();
				Transform handleTransform = gridUpdater.transform;
				Quaternion handleRotation = gridUpdater.transform.rotation;
				Vector3 cellRotation = gridUpdater.CellRotation; //handleTransform.TransformPoint(gridUpdater.CornerPos);

				// FIXME: rotation handle position
				cellRotation = Handles.DoRotationHandle(Quaternion.Euler(cellRotation), gridUpdater.transform.position).eulerAngles;
				if (EditorGUI.EndChangeCheck()) {
					Undo.RecordObject(gridUpdater, "Move grid corner");
					EditorUtility.SetDirty(gridUpdater);
					// item.Item2.RearLeftBone.position = handleTransform.InverseTransformPoint(p0l);
					// gridUpdater.CornerPos = handleTransform.InverseTransformPoint(cellRotation);
					gridUpdater.CellRotation = handleTransform.InverseTransformDirection(cellRotation);
					if (autoUpdate)
						gridUpdater.UpdateCells();
				}

				break;
		}

		// */

	}

	public override void OnInspectorGUI() {
		// Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
		serializedObject.Update();

		CenterlineScript centerlineScript = (CenterlineScript)target;

		EditorGUI.BeginChangeCheck();

		// EditorGUILayout.PropertyField(controlPoints);
		for (int i = 0; i < centerlineScript.ControlPoints.Count; i++) {
			EditorGUILayout.BeginHorizontal();
			centerlineScript.ControlPoints[i] = EditorGUILayout.Vector3Field($"p{i}", centerlineScript.ControlPoints[i]);
			if (GUILayout.Button("Remove")) {
				centerlineScript.ControlPoints.RemoveAt(i);
			}
			EditorGUILayout.EndHorizontal();
		}
		if (GUILayout.Button("Add")) {
			if (centerlineScript.ControlPoints.Count > 0) {
				centerlineScript.ControlPoints.Add(centerlineScript.ControlPoints[centerlineScript.ControlPoints.Count - 1]);
			} else {
				centerlineScript.ControlPoints.Add(Vector3.zero);
			}
		}

		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(centerlineScript, "Changed centerline control points");
			EditorUtility.SetDirty(centerlineScript);
		}


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
