using UnityEditor;
using UnityEngine;
using System.Collections;

// Custom Editor using SerializedProperties.
// Automatic handling of multi-object editing, undo, and Prefab overrides.
[CustomEditor(typeof(CenterlineScript))]
[CanEditMultipleObjects]
public class CenterlineEditorScript : Editor {
	SerializedProperty controlPoints;


	void OnEnable() {
		controlPoints = serializedObject.FindProperty("ControlPoints");
	}

	private void OnSceneGUI() {

		CenterlineScript centerlineScript = (CenterlineScript)target;

		Handles.color = Color.white;

		// TODO: draw lines along sides of grid
		for (int i = 1; i < centerlineScript.LinePoints.Count; i++) {

			Handles.DrawLine(
				centerlineScript.transform.TransformPoint(centerlineScript.LinePoints[i - 1]),
				centerlineScript.transform.TransformPoint(centerlineScript.LinePoints[i])
			);

		}
		// Handles.DrawLine(gridUpdater.transform.position, gridUpdater.);
		// Handles.DrawLine(gridUpdater.transform.position, gridUpdater.);
		// Handles.DrawLine(gridUpdater.transform.position, gridUpdater.);

		for (int i = 0; i < centerlineScript.ControlPoints.Count; i++) {

			EditorGUI.BeginChangeCheck();

			Transform handleTransform = centerlineScript.transform;
			Quaternion handleRotation = centerlineScript.transform.rotation;
			Vector3 pos = handleTransform.TransformPoint(centerlineScript.ControlPoints[i]);
			pos = Handles.DoPositionHandle(pos, handleRotation);

			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(centerlineScript, "Moved centerline control point");
				EditorUtility.SetDirty(centerlineScript);
				// item.Item2.RearLeftBone.position = handleTransform.InverseTransformPoint(p0l);
				centerlineScript.ControlPoints[i] = handleTransform.InverseTransformPoint(pos);
				centerlineScript.GenerateLinePoints();
			}
		}
		/*

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

		centerlineScript.Resolution = EditorGUILayout.IntField("Resolution/Line Count", centerlineScript.Resolution);

		if (EditorGUI.EndChangeCheck()) {
			centerlineScript.GenerateLinePoints();
			Undo.RecordObject(centerlineScript, "Changed centerline control points");
			EditorUtility.SetDirty(centerlineScript);
		}


		// Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();
	}

}
