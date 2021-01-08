using UnityEditor;
using UnityEngine;
using System.Collections;

// Custom Editor using SerializedProperties.
// Automatic handling of multi-object editing, undo, and Prefab overrides.
[CustomEditor(typeof(CenterlineScript))]
[CanEditMultipleObjects]
public class CenterlineEditorScript : Editor {
	SerializedProperty controlPoints;

	float handleSize = .04f;
	// int maxResolution = 100;
	bool overrideResolutionMax = false;

	void OnEnable() {
		controlPoints = serializedObject.FindProperty("ControlPoints");
	}

	private void OnSceneGUI() {

		CenterlineScript centerlineScript = (CenterlineScript)target;

		Handles.color = Color.white;

		for (int i = 1; i < centerlineScript.LinePoints.Count; i++) {

			Handles.DrawLine(
				centerlineScript.transform.TransformPoint(centerlineScript.LinePoints[i - 1]),
				centerlineScript.transform.TransformPoint(centerlineScript.LinePoints[i])
			);

		}
		// Handles.DrawLine(gridUpdater.transform.position, gridUpdater.);
		// Handles.DrawLine(gridUpdater.transform.position, gridUpdater.);
		// Handles.DrawLine(gridUpdater.transform.position, gridUpdater.);

		Handles.color = Color.blue;

		for (int i = 0; i < centerlineScript.ControlPoints.Count; i++) {

			EditorGUI.BeginChangeCheck();

			Transform handleTransform = centerlineScript.transform;
			Quaternion handleRotation = centerlineScript.transform.rotation;
			Vector3 pos = handleTransform.TransformPoint(centerlineScript.ControlPoints[i]);
			pos = Handles.FreeMoveHandle(pos, handleRotation, handleSize, Vector3.one, Handles.DotHandleCap);

			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(centerlineScript, "Moved centerline control point");
				EditorUtility.SetDirty(centerlineScript);
				centerlineScript.ControlPoints[i] = handleTransform.InverseTransformPoint(pos);
				centerlineScript.GenerateLinePoints();
			}
			// IDEA: create new control point if start or end point is moved too far away from the next/previous point
		}

		Handles.color = Color.gray;

		for (int i = 0; i < centerlineScript.LinePoints.Count; i++) {

			EditorGUI.BeginChangeCheck();

			Transform handleTransform = centerlineScript.transform;
			Quaternion handleRotation = centerlineScript.transform.rotation;
			Vector3 pos = handleTransform.TransformPoint(centerlineScript.LinePoints[i]);
			pos = Handles.FreeMoveHandle(pos, handleRotation, handleSize, Vector3.one, Handles.DotHandleCap);

			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(centerlineScript, "Moved centerline control point");
				EditorUtility.SetDirty(centerlineScript);
				centerlineScript.LinePoints[i] = handleTransform.InverseTransformPoint(pos);
			}
			// IDEA: create new control point if start or end point is moved too far away from the next/previous point
		}


	}

	public override void OnInspectorGUI() {
		serializedObject.Update();

		CenterlineScript centerlineScript = (CenterlineScript)target;

		EditorGUIUtility.labelWidth = 30;

		EditorGUI.BeginChangeCheck();

		for (int i = 0; i < centerlineScript.ControlPoints.Count; i++) {
			EditorGUILayout.BeginHorizontal();
			centerlineScript.ControlPoints[i] = EditorGUILayout.Vector3Field($"p{i}", centerlineScript.ControlPoints[i]);
			if (GUILayout.Button("Remove", GUILayout.Width(70))) {
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

		EditorGUIUtility.labelWidth = 135;

		// EditorGUILayout.BeginHorizontal();
		if (overrideResolutionMax) {
			centerlineScript.Resolution = EditorGUILayout.IntField("Resolution/Line Count", centerlineScript.Resolution);
		} else {
			centerlineScript.Resolution = EditorGUILayout.IntSlider("Resolution/Line Count", centerlineScript.Resolution, 0, 100);
		}
		// FIXME: deselecting and selecting again will set the resolution back to below/at limit if it was above limit
		// overrideResolutionMax = EditorGUILayout.Toggle("Override max resolution", overrideResolutionMax);
		// EditorGUILayout.EndHorizontal();


		if (EditorGUI.EndChangeCheck()) {
			centerlineScript.GenerateLinePoints();
			Undo.RecordObject(centerlineScript, "Changed centerline control points");
			EditorUtility.SetDirty(centerlineScript);
		}

		float oldHandleSize = handleSize;
		handleSize = EditorGUILayout.Slider("Handle Size", handleSize, 0.01f, .5f);

		if (oldHandleSize != handleSize) {
			SceneView.RepaintAll();
		}

		serializedObject.ApplyModifiedProperties();
	}

}
